// This is a Bootcheck service that checks if all the necessary environment variables are set and if the service account key file is valid.
// System will stop if bootcheck fails.

using Google.Apis.Auth.OAuth2;

namespace Backend.Services {
    public static class Bootcheck {
        public static void Run() {
            var missingVariables = new List<string>();
            var environmentVariables = new string[] {
                "EMAILER_ENABLED",
                "EMAIL_SENDER",
                "EMAIL_PASSWORD",
                "HTTP_URL",
                "HTTPS_URL",
                "SMS_ENABLED",
                "TWILIO_ACCOUNT_SID",
                "TWILIO_AUTH_TOKEN",
                "TWILIO_REGISTERED_PHONE_NUMBER",
                "GOOGLE_APPLICATION_CREDENTIALS",
                "FIREBASE_STORAGE_BUCKET_URL"
            };

            missingVariables = environmentVariables
                .Where(var => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(var)))
                .ToList();

            if (missingVariables.Any()) {
                Console.WriteLine("");
                Console.WriteLine($"Environment variables {string.Join(", ", missingVariables)} not set.");
                Environment.Exit(1);
            } else {
                var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

                if (!File.Exists(credentialsPath)) {
                    Console.WriteLine("");
                    Console.WriteLine("Service account key file not found.");
                    Environment.Exit(1);
                } else {
                    try {
                        GoogleCredential.FromFile(credentialsPath);
                        Console.WriteLine("");
                        Console.WriteLine("BOOTCHECK COMPLETE. SYSTEM READY.");
                        Console.WriteLine("");
                    } catch (Exception ex) {
                        Console.WriteLine("");
                        Console.WriteLine("Service account key file is invalid. It may have been tampered with.");
                        Logger.Log("BOOTCHECK - Service account key file is invalid. ERROR: " + ex.Message);
                        Environment.Exit(1);
                    }
                }
            }
        }
    }
}