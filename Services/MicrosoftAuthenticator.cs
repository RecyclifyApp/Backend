using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public static class MSAuth
{
    private static readonly HttpClient _httpClient = new HttpClient();
    public static string RapidAPIKey { get; private set; }
    public const string RapidAPIHost = "microsoft-authenticator.p.rapidapi.com";
    public const string BaseURL = "https://microsoft-authenticator.p.rapidapi.com/";

    public static bool CheckPermissions()
    {
        var enabled = Environment.GetEnvironmentVariable("MSAuthEnabled");
        return enabled == "True";
    }

    public static string Setup()
    {
        if (!CheckPermissions())
            return "ERROR: MSAuth is not allowed to operate.";

        var apiKey = Environment.GetEnvironmentVariable("RapidAPIKey");
        if (string.IsNullOrEmpty(apiKey))
            return "ERROR: RapidAPIKey environment variable not found in .env file.";

        RapidAPIKey = apiKey;
        return "Success";
    }

    public static object NewSecret()
    {
        if (!CheckPermissions())
            return "ERROR: MSAuth is not allowed to operate.";

        try
        {
            var url = BaseURL + "new_v2/";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("X-RapidAPI-Key", RapidAPIKey);
            request.Headers.Add("X-RapidAPI-Host", RapidAPIHost);

            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception e)
        {
            return $"ERROR: Failed to create and save new secret; error: {e.Message}";
        }
    }

    public static object Enroll(string account, string issuer, string secret)
    {
        if (!CheckPermissions())
            return "ERROR: MSAuth is not allowed to operate.";

        if (account.Contains(" ") || issuer.Contains(" "))
            return "ERROR: Account and issuer values cannot have spaces.";

        try
        {
            var url = BaseURL + "enroll/";
            var formData = new Dictionary<string, string>
            {
                { "secret", secret },
                { "account", account },
                { "issuer", issuer }
            };
            var content = new FormUrlEncodedContent(formData);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Add("X-RapidAPI-Key", RapidAPIKey);
            request.Headers.Add("X-RapidAPI-Host", RapidAPIHost);

            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            
            var responseText = response.Content.ReadAsStringAsync().Result;
            return responseText.Length > 0 ? responseText[..^1] : responseText;
        }
        catch (Exception e)
        {
            return $"ERROR: Failed to get enrollment QR URL from API; error: {e.Message}";
        }
    }

    public static object Validate(string code, string secret)
    {
        if (!CheckPermissions())
            return "ERROR: MSAuth is not allowed to operate.";

        try
        {
            var url = BaseURL + "validate/";
            var formData = new Dictionary<string, string>
            {
                { "secret", secret },
                { "code", code }
            };
            var content = new FormUrlEncodedContent(formData);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Add("X-RapidAPI-Key", RapidAPIKey);
            request.Headers.Add("X-RapidAPI-Host", RapidAPIHost);

            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStringAsync().Result == "True";
        }
        catch (Exception e)
        {
            return $"ERROR: Failed to validate the OTP code; error: {e.Message}";
        }
    }
}