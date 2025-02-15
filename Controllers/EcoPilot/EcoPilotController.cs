using Microsoft.AspNetCore.Mvc;
using Backend.Services; // Assuming your existing OpenAIChatService is in this namespace

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
            // Initialize your recycling-related documents.
            _documents = LoadRecyclingDocuments();
        }

        private List<Document> LoadRecyclingDocuments()
        {
            // Replace these with your actual recycling knowledge base documents.
            return new List<Document>
            {
                new Document("Recycling helps reduce waste and conserve natural resources. It is an essential part of a sustainable future.", new Dictionary<string,string>{{"source", "recycling1"}}),
                new Document("Plastic recycling is crucial to reduce ocean pollution and protect marine life.", new Dictionary<string,string>{{"source", "recycling2"}}),
                new Document("Paper recycling saves trees and reduces energy consumption in manufacturing.", new Dictionary<string,string>{{"source", "recycling3"}})
                // Add more documents as needed.
            };
        }

        // Dummy search: In production, replace with proper embeddings and cosine similarity.
        public Task<List<Document>> SearchAsync(string query, int k)
        {
            // For demonstration, simply return the first k documents.
            return Task.FromResult(_documents.Take(k).ToList());
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
    public class EcoPilotController : ControllerBase
    {
        private readonly RagOpenAIChatService _ragChatService;

        public EcoPilotController(RagOpenAIChatService ragChatService)
        {
            _ragChatService = ragChatService;
        }

        [HttpPost("prompt")]
        public async Task<IActionResult> QueryCycloBotWithUserPrompt([FromBody] UserPromptRequest request)
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