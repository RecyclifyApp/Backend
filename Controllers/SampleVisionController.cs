// This controller is just to illustrate how to use the various Utility Tools using the Utilities class.

// using Backend.Services;
// using Microsoft.AspNetCore.Mvc;

// namespace Backend.Controllers {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class SampleVisionController : ControllerBase {
//         [HttpPost("upload")]
//         public IResult UploadImage(IFormFile file)
//         {
//             if (file == null || string.IsNullOrEmpty(file.FileName))
//             {
//                 return Results.BadRequest(new { error = "No file uploaded" });
//             }

//             string tempFilePath = CompVision.SaveTempFile(file);
//             string optimizedFilePath = CompVision.OptimizeImage(tempFilePath);

//             var analysisResult = CompVision.GetDetectedObjects(optimizedFilePath);

//             if (analysisResult == null)
//             {
//                 return Results.BadRequest(new { error = "Error analyzing image" });
//             }

//             var detectedObjects = analysisResult;
//             if (detectedObjects == null || !detectedObjects.Any())
//             {
//                 return Results.Ok(new { result = "No", category = "No match", items = new List<string>() });
//             }

//             var recyclableCategories = CompVision.GetRecyclableCategories(detectedObjects);

//             if (recyclableCategories != null)
//             {
//                 var bestCategory = CompVision.GetBestFittingCategory(file.FileName, detectedObjects, recyclableCategories);
//                 if (bestCategory != null)
//                 {
//                     return Results.Ok(new { result = "Yes", category = bestCategory, items = detectedObjects });
//                 }
//             }

//             return Results.Ok(new { result = "No", category = "No match", items = detectedObjects });
//         }

//         [HttpPost("analyze")]
//         public static IResult AnalyzeImage(IFormFile file)
//         {
//             if (file == null || string.IsNullOrEmpty(file.FileName))
//             {
//                 return Results.BadRequest(new { error = "No file uploaded" });
//             }

//             string tempFilePath = CompVision.SaveTempFile(file);
//             string optimizedFilePath = CompVision.OptimizeImage(tempFilePath);

//             var analysisResult = CompVision.GetDetectedObjects(optimizedFilePath);

//             if (analysisResult == null)
//             {
//                 return Results.BadRequest(new { error = "Error analyzing image" });
//             }

//             return Results.Ok(new { detectedObjects = analysisResult });
//         }
//     }
// }

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Vision.V1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Newtonsoft.Json;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleVisionController : ControllerBase
    {
        // Load category maps from JSON files directly
        private static readonly Dictionary<string, List<string>> categoryMap = LoadJson<Dictionary<string, List<string>>>("data/category_map.json");

        private static readonly Dictionary<string, List<string>> secondaryMap = LoadJson<Dictionary<string, List<string>>>("data/secondary_map.json");

        private static T LoadJson<T>(string path)
        {
            using (var reader = new StreamReader(path))
            {
                var result = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                if (result == null)
                {
                    throw new InvalidOperationException("Deserialization returned null");
                }
                return result;
            }
        }

        private string OptimizeImage(string imagePath, (int width, int height) maxSize = default)
        {
            if (maxSize == default) maxSize = (800, 800);

            using (var image = SixLabors.ImageSharp.Image.Load(imagePath)) // Corrected to ImageSharp's Load method
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxSize.width, maxSize.height),
                    Mode = ResizeMode.Max
                }));

                var jpegPath = imagePath + ".jpg";
                image.Save(jpegPath, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 85 });
                return jpegPath;
            }
        }


        private async Task<List<string>> GetDetectedObjectsAsync(string imagePath)
        {
            var client = ImageAnnotatorClient.Create();
            var image = await Google.Cloud.Vision.V1.Image.FromFileAsync(imagePath);
            var response = await client.DetectLocalizedObjectsAsync(image);
            return response?.Select(obj => obj.Name).ToList() ?? new List<string>();
        }

        private List<string> GetRecyclableCategories(List<string> detectedObjects)
        {
            return categoryMap
                .Where(category => category.Value.Any(term => detectedObjects.Contains(term)))
                .Select(category => category.Key)
                .ToList();
        }

        private async Task<string> GetBestFittingCategory(string fileName, string imagePath,
            List<string> detectedObjects, List<string> matchedCategories)
        {
            var categoryMatchCount = matchedCategories.ToDictionary(
                category => category,
                category => categoryMap[category].Count(term => detectedObjects.Contains(term))
            );

            var maxCount = categoryMatchCount.Values.Max();
            var tiedCategories = categoryMatchCount
                .Where(kvp => kvp.Value == maxCount)
                .Select(kvp => kvp.Key)
                .ToList();

            if (tiedCategories.Count > 1)
            {
                var resolvedCategory = await GranularAnalysisToResolveTie(fileName, imagePath, tiedCategories);
                return resolvedCategory ?? string.Empty;
            }

            return tiedCategories.First();
        }

        private async Task<string?> GranularAnalysisToResolveTie(string fileName, string imagePath,
            List<string> tiedCategories)
        {
            var client = new ImageAnnotatorClientBuilder().Build();

            var image = await Google.Cloud.Vision.V1.Image.FromFileAsync(imagePath);
            var response = await client.DetectLabelsAsync(image);

            var labels = response.Select(label => label.Description).ToList();
            var labelMatchCount = tiedCategories.ToDictionary(
                category => category,
                category => categoryMap[category].Count(term => labels.Contains(term))
            );

            var bestCategory = labelMatchCount.OrderByDescending(kvp => kvp.Value).First();
            return bestCategory.Value > 0 ? bestCategory.Key : null;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            var tempPath = Path.GetTempFileName();
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var optimizedImagePath = OptimizeImage(tempPath);

            try
            {
                var detectedObjects = await GetDetectedObjectsAsync(optimizedImagePath);

                if (!detectedObjects.Any())
                    return Ok(new { result = "No", category = "No match", items = new List<string>() });

                var recyclableCategories = GetRecyclableCategories(detectedObjects);

                if (recyclableCategories.Any())
                {
                    var bestCategory = await GetBestFittingCategory(file.FileName, tempPath,
                        detectedObjects, recyclableCategories);

                    if (bestCategory != null)
                    {
                        return Ok(new { result = "Yes", category = bestCategory, items = detectedObjects });
                    }
                }

                return Ok(new { result = "No", category = "No match", items = detectedObjects });
            }
            finally
            {
                if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
                if (System.IO.File.Exists(optimizedImagePath)) System.IO.File.Delete(optimizedImagePath);
            }
        }
    }
}
