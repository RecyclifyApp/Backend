using Google.Cloud.Vision.V1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Newtonsoft.Json;

namespace Backend.Services {
    public class CompVision {
        private static bool ContextChecked = false;
        
        private static bool CheckPermission() {
            return Environment.GetEnvironmentVariable("COMPVISION_ENABLED") == "True";
        }

        private static void CheckContext() {
            if (!CheckPermission()) {
                throw new Exception("ERROR: CompVision is not enabled.");
            }

            ContextChecked = true;
        }

        private static readonly Dictionary<string, List<string>> categoryMap = LoadJson<Dictionary<string, List<string>>>("data/category_map.json");

        private static T LoadJson<T>(string path) {
            using (var reader = new StreamReader(path)) {
                var result = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                if (result == null) {
                    throw new InvalidOperationException("Deserialization returned null");
                }
                return result;
            }
        }

        private static string EnhanceAndOptimizeImage(string imagePath, (int width, int height) maxSize = default) {
            if (maxSize == default) maxSize = (800, 800);

            using (var image = SixLabors.ImageSharp.Image.Load(imagePath)) {
                image.Mutate(x => x
                    .Resize(new ResizeOptions {
                        Size = new Size(maxSize.width, maxSize.height),
                        Mode = ResizeMode.Max
                    })
                    .GaussianSharpen(0.5f)
                    .AutoOrient());

                var jpegPath = imagePath + ".jpg";
                image.Save(jpegPath, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 90 });
                return jpegPath;
            }
        }

        private static async Task<List<string>> GetEnhancedDetectedObjectsAsync(string imagePath) {
            var client = ImageAnnotatorClient.Create();
            var image = await Google.Cloud.Vision.V1.Image.FromFileAsync(imagePath);

            // Multi-detection of objects, labels and text
            var objectResponse = await client.DetectLocalizedObjectsAsync(image);
            var labelResponse = await client.DetectLabelsAsync(image);
            var textResponse = await client.DetectTextAsync(image);

            // Combine results
            var detectedObjects = objectResponse.Select(obj => obj.Name).ToList();
            var detectedLabels = labelResponse.Select(label => label.Description).ToList();
            var detectedText = textResponse.Select(text => text.Description).ToList();

            return detectedObjects
                .Concat(detectedLabels)
                .Concat(detectedText)
                .Distinct()
                .ToList();
        }

        private static List<string> GetWeightedRecyclableCategories(List<string> detectedObjects) {
            var normalizedDetectedObjects = detectedObjects
                .Select(obj => obj.ToLower().Trim())
                .ToList();

            var categoriesWithScores = categoryMap
                .Select(category => new {
                    Category = category.Key,
                    Score = category.Value.Count(term => normalizedDetectedObjects.Contains(term.ToLower().Trim()))
                })
                .ToList();

            var maxScore = categoriesWithScores.Max(x => x.Score);

            return categoriesWithScores
                .Where(x => x.Score == maxScore && x.Score > 0)
                .Select(x => x.Category)
                .ToList();
        }


        private static async Task<string?> GranularAnalysisWithConfidence(string imagePath, List<string> tiedCategories) {
            var client = ImageAnnotatorClient.Create();
            var image = await Google.Cloud.Vision.V1.Image.FromFileAsync(imagePath);

            var response = await client.DetectLabelsAsync(image);

            var labelMatchCount = tiedCategories.ToDictionary(
                category => category,
                category => categoryMap[category]
                    .Sum(term => response.Where(label => label.Description == term)
                    .Sum(label => label.Score)) // Weigh by confidence
            );

            var bestCategory = labelMatchCount.OrderByDescending(kvp => kvp.Value).First();
            return bestCategory.Value > 0 ? bestCategory.Key : null;
        }

        public static async Task<object> Recognise(IFormFile file) {
            CheckContext();
            if (!ContextChecked) {
                return "ERROR: System context was not checked before proce4ssing image for recognition. Skipping image recognition.";
            }

            var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);
            var stream = file.OpenReadStream();

            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write)) {
                await stream.CopyToAsync(fileStream);
            }

            string optimizedImagePath;
            
            try {
                optimizedImagePath = EnhanceAndOptimizeImage(tempPath);
            } catch (UnknownImageFormatException ex) {
                Logger.Log($"IMAGE PROCESSING ERROR: {ex.Message}");
                throw new Exception("Invalid image format. Please upload a valid image file.");
            } catch (Exception ex) {
                Logger.Log($"GENERAL ERROR: {ex.Message}");
                throw new Exception("An error occurred while processing the image.");
            }

            try {
                var detectedObjects = await GetEnhancedDetectedObjectsAsync(optimizedImagePath);

                if (!detectedObjects.Any()) {
                    return new { result = "No", category = "No match", items = new List<string>() };
                }

                var recyclableCategories = GetWeightedRecyclableCategories(detectedObjects);

                if (recyclableCategories.Any() && recyclableCategories.Count > 0) {
                    if (recyclableCategories.Count == 1) {
                        // Single best category
                        var bestCategory = recyclableCategories.First();
                        return new { result = "Yes", category = bestCategory, items = detectedObjects };
                    } else {
                        var granularAnalysisResult = await GranularAnalysisWithConfidence(optimizedImagePath, recyclableCategories);

                        if (granularAnalysisResult != null) {
                            return new { result = "Yes", category = granularAnalysisResult, items = detectedObjects };
                        } else {
                            // Fallback to the first category if GranularAnalysisWithConfidence fails
                            return new { result = "Yes", category = recyclableCategories.First(), items = detectedObjects };
                        }
                    }
                } else {
                    return new { result = "No", category = "No match", items = detectedObjects };
                }
            } finally {
                if (File.Exists(tempPath)) {
                    File.Delete(tempPath);
                }
                if (File.Exists(optimizedImagePath)) {
                    File.Delete(optimizedImagePath);
                }
            }
        }
    }
}
