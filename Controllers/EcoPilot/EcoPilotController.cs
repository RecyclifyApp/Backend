using Microsoft.AspNetCore.Mvc;
using Backend.Services; // Assuming your existing OpenAIChatService is in this namespace
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using Backend.Filters;
using Microsoft.AspNetCore.Authorization;

namespace EcoPilotApp
{
    // A simple Document class holding content and metadata.
    public class Document
    {
        public string Content { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public Document(string content, Dictionary<string, string>? metadata = null)
        {
            Content = content;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }

    // Interface for a vector store service that retrieves relevant documents.
    public interface IVectorStoreService
    {
        Task<List<Document>> SearchAsync(string query, int k);
    }

    // A dummy vector store service implementation.
    public class VectorStoreService : IVectorStoreService
    {
        private readonly List<Document> _documents;

        public VectorStoreService()
        {
            // Load documents from CSV file
            _documents = LoadRecyclingDocuments("data/Recyclify.csv");
        }

        private List<Document> LoadRecyclingDocuments(string filePath)
        {
            var documents = new List<Document>();

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                };

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    if (csv.Read())
                    {
                        csv.ReadHeader(); // Ensure headers are read first
                    }

                    while (csv.Read())
                    {
                        var content = csv.GetField<string>("content");
                        var source = csv.GetField<string>("source");

                        if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(source))
                        {
                            documents.Add(new Document(content, new Dictionary<string, string> { { "source", source } }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading CSV file: {ex.Message}");
            }

            return documents;
        }

        public Task<List<Document>> SearchAsync(string query, int k)
        {
            // Rank documents by keyword match count
            var rankedDocs = _documents
                .Select(doc => new
                {
                    Document = doc,
                    Score = query.Split(' ').Count(word => doc.Content.Contains(word, StringComparison.OrdinalIgnoreCase))
                })
                .OrderByDescending(d => d.Score)
                .Take(k)
                .Select(d => d.Document)
                .ToList();

            return Task.FromResult(rankedDocs);
        }
    }
    // RAG-enabled chat service: Retrieves relevant documents and augments the user's prompt.
    public class RagOpenAIChatService
    {
        private readonly OpenAIChatService _chatService; // Your existing service
        private readonly IVectorStoreService _vectorStoreService;

        public RagOpenAIChatService(OpenAIChatService chatService, IVectorStoreService vectorStoreService)
        {
            _chatService = chatService;
            _vectorStoreService = vectorStoreService;
        }

        public async Task<string> PromptAsync(string userPrompt)
        {
            // Retrieve context documents relevant to recycling.
            var docs = await _vectorStoreService.SearchAsync(userPrompt, k: 3);
            // Combine retrieved documents into a single background string.
            var context = string.Join("\n\n", docs.Select(d => d.Content));

            // Build an augmented prompt that includes background context.
            var augmentedPrompt = $"You are an expert on recycling and sustainability. Use the following background information to help answer the user query.\n\n" +
                                  $"Background:\n{context}\n\n" +
                                  $"User Query: {userPrompt}";

            // Call your existing OpenAIChatService with the augmented prompt.
            var answer = await _chatService.PromptAsync(augmentedPrompt);
            return answer;
        }
    }

    // Model for the user's prompt.
    public class UserPromptRequest
    {
        public string UserPrompt { get; set; } = string.Empty;
    }

    // API Controller that uses the RAG-enabled chat service.
    [ApiController]
    [Route("api/chat-completion")]
    [ServiceFilter(typeof(CheckSystemLockedFilter))]
    public class EcoPilotController : ControllerBase
    {
        private readonly RagOpenAIChatService _ragChatService;

        public EcoPilotController(RagOpenAIChatService ragChatService)
        {
            _ragChatService = ragChatService;
        }

        [HttpPost("prompt")]
        [Authorize]
        public async Task<IActionResult> QueryEcoPilotWithUserPrompt([FromBody] UserPromptRequest request)
        {
            if (string.IsNullOrEmpty(request.UserPrompt))
            {
                return BadRequest(new { error = "UERROR: User Prompt is required" });
            }

            var response = await _ragChatService.PromptAsync(request.UserPrompt);
            return Ok(response);
        }
    }
}