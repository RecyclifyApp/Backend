using System.Text.Json;

namespace Backend.Services {
    public class Captcha {
        private readonly HttpClient _httpClient;
        private readonly string _recaptchaSecretKey;

        public Captcha(HttpClient httpClient) {
            _httpClient = httpClient;

            // Load environment variables from .env file
            _recaptchaSecretKey = Environment.GetEnvironmentVariable("CAPTCHA_SECRET_KEY") ?? throw new InvalidOperationException("CAPTCHA_SECRET_KEY is not set in .env file.");
        }

        public async Task<(bool success, double score)> ValidateCaptchaAsync(string recaptchaResponse) {
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("secret", _recaptchaSecretKey),
                new KeyValuePair<string, string>("response", recaptchaResponse)
            });

            try {
                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var responseString = await response.Content.ReadAsStringAsync();

                var jsonDocument = JsonDocument.Parse(responseString);
                var successValue = jsonDocument.RootElement.GetProperty("success").GetBoolean();
                var scoreValue = jsonDocument.RootElement.GetProperty("score").GetDouble();

                return (successValue, scoreValue);
            } catch (Exception) {
                return (false, 0);
            }
        }
    }
}