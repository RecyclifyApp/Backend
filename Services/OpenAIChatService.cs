using System.Net.Http.Headers;
using System.Text.Json;

namespace Backend.Services {
    public class OpenAIChatService {
        public async Task<string> PromptAsync(string messagePrompt) {
            var _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;

            if (string.IsNullOrEmpty(_apiKey)) {
                throw new InvalidOperationException("OpenAI API key is missing. Please check your .env file.");
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            // Correct request body for OpenAI Chat Completions API
            var requestBody = new {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = messagePrompt }
                },
                max_tokens = 150,
                temperature = 0.7
            };

            var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

            if (!response.IsSuccessStatusCode) {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI API error: {error}");
            }

            var responseData = await response.Content.ReadAsStringAsync();

            // Parse and extract the assistant's reply
            using var jsonDoc = JsonDocument.Parse(responseData);
            var message = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return message ?? "ERROR: Failed to process response";
        }
    }
}