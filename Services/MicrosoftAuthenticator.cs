using Backend;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services {
    public class MSAuth {
        private readonly MyDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string RapidAPIKey;
        public MSAuth(MyDbContext context, HttpClient httpClient) {
            _context = context;
            _httpClient = httpClient;
            RapidAPIKey = Environment.GetEnvironmentVariable("RapidAPIKey") 
                ?? throw new InvalidOperationException("ERROR: RAPIDAPI_KEY environment variable not set.");
        }
        public const string RapidAPIHost = "microsoft-authenticator.p.rapidapi.com";
        public const string BaseURL = "https://microsoft-authenticator.p.rapidapi.com/";

        private async Task CheckPermissionAsync() {
            var MsAuthEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "MSAuthEnabled");
            if (MsAuthEnabled == null) {
                throw new InvalidOperationException("ERROR: MSAuthEnabled configuration is missing.");
            }
            if (MsAuthEnabled.Value == "false") {
                throw new Exception("ERROR: Microsoft Authenticator Service is Disabled.");
            }
        }

        public async Task<object> NewSecret() {
            await CheckPermissionAsync();
            try {
                var url = BaseURL + "new_v2/";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("X-RapidAPI-Key", RapidAPIKey);
                request.Headers.Add("X-RapidAPI-Host", RapidAPIHost);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            } catch (Exception e) {
                return $"ERROR: {e.Message}";
            }
        }

        public async Task<object> Enroll(string account, string issuer, string secret) {
            await CheckPermissionAsync();
            try {
                var url = BaseURL + "enroll/";
                var formData = new Dictionary<string, string> {
                    { "secret", secret }, { "account", account }, { "issuer", issuer }
                };
                var content = new FormUrlEncodedContent(formData);

                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                request.Headers.Add("X-RapidAPI-Key", RapidAPIKey);
                request.Headers.Add("X-RapidAPI-Host", RapidAPIHost);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            } catch (Exception e) {
                return $"ERROR: {e.Message}";
            }
        }

        public async Task<object> Validate(string code, string secret) {
            await CheckPermissionAsync();
            try {
                var url = BaseURL + "validate/";
                var formData = new Dictionary<string, string> { { "secret", secret }, { "code", code } };
                var content = new FormUrlEncodedContent(formData);

                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                request.Headers.Add("X-RapidAPI-Key", RapidAPIKey);
                request.Headers.Add("X-RapidAPI-Host", RapidAPIHost);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();
                return bool.TryParse(responseText, out bool result) ? result : (object)$"ERROR: Invalid response {responseText}";
            } catch (Exception e) {
                return $"ERROR: {e.Message}";
            }
        }
    }
}