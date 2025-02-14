using Backend;
using Microsoft.EntityFrameworkCore;

public class MSAuth {
    private readonly MyDbContext _context;
    public MSAuth(MyDbContext context) {
        _context = context;
    }
    private static readonly HttpClient _httpClient = new HttpClient();
    public static string RapidAPIKey = Environment.GetEnvironmentVariable("RAPIDAPI_KEY") ?? throw new InvalidOperationException("ERROR: RAPIDAPI_KEY environment variable not set.");
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

            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        } catch (Exception e) {
            return $"ERROR: Failed to create and save new secret; error: {e.Message}";
        }
    }

    public async Task<object> Enroll(string account, string issuer, string secret) {
        await CheckPermissionAsync();

        if (account.Contains(" ") || issuer.Contains(" "))
            return "ERROR: Account and issuer values cannot have spaces.";

        try {
            var url = BaseURL + "enroll/";
            var formData = new Dictionary<string, string> {
                { "secret", secret },
                { "account", account },
                { "issuer", issuer }
            };
            var content = new FormUrlEncodedContent(formData);

            var request = new HttpRequestMessage(HttpMethod.Post, url) {
                Content = content
            };
            request.Headers.Add("X-RapidAPI-Key", RapidAPIKey);
            request.Headers.Add("X-RapidAPI-Host", RapidAPIHost);

            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            
            var responseText = response.Content.ReadAsStringAsync().Result;
            return responseText.Length > 0 ? responseText[..^1] : responseText;
        } catch (Exception e) {
            return $"ERROR: Failed to get enrollment QR URL from API; error: {e.Message}";
        }
    }

    public async Task<object> Validate(string code, string secret) {
        await CheckPermissionAsync();

        try {
            var url = BaseURL + "validate/";
            var formData = new Dictionary<string, string> {
                { "secret", secret },
                { "code", code }
            };
            var content = new FormUrlEncodedContent(formData);

            var request = new HttpRequestMessage(HttpMethod.Post, url) {
                Content = content
            };
            request.Headers.Add("X-RapidAPI-Key", RapidAPIKey);
            request.Headers.Add("X-RapidAPI-Host", RapidAPIHost);

            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStringAsync().Result == "True";
        } catch (Exception e) {
            return $"ERROR: Failed to validate the OTP code; error: {e.Message}";
        }
    }
}