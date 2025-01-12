// This is a Bootcheck service that checks if all the necessary environment variables are set and if the service account key file is valid.
// System will stop if bootcheck fails.

using Google.Apis.Auth.OAuth2;

namespace Backend.Services {
    public static class Bootcheck {
        public static void Run() {
            var missingVariables = new List<string>();
            var environmentVariables = new string[] {
                "EMAILER_ENABLED",
                "SMS_ENABLED",
                "COMPVISION_ENABLED",
                "EMAIL_SENDER",
                "EMAIL_PASSWORD",
                "HTTP_URL",
                "HTTPS_URL",
                "TWILIO_ACCOUNT_SID",
                "TWILIO_AUTH_TOKEN",
                "TWILIO_REGISTERED_PHONE_NUMBER",
                "GOOGLE_APPLICATION_CREDENTIALS",
                "FIREBASE_APPLICATION_CREDENTIALS",
                "FIREBASE_STORAGE_BUCKET_URL",
                "CLOUDSQL_CONNECTION_STRING"
            };

            missingVariables = environmentVariables
                .Where(var => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(var)))
                .ToList();

            if (missingVariables.Any()) {
                Console.WriteLine("");
                Console.WriteLine($"Environment variables {string.Join(", ", missingVariables)} not set.");
                Environment.Exit(1);
            } else {
                var firebaseCredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
                var gcpCredentialsPath = Environment.GetEnvironmentVariable("FIREBASE_APPLICATION_CREDENTIALS");

                if (!File.Exists(firebaseCredentialsPath) || !File.Exists(gcpCredentialsPath)) {
                    Console.WriteLine("");
                    Console.WriteLine("Service account key files missing.");
                    Environment.Exit(1);
                } else {
                    try {
                        GoogleCredential.FromFile(firebaseCredentialsPath);
                        GoogleCredential.FromFile(gcpCredentialsPath);
                        Console.WriteLine("");
                        Console.WriteLine("BOOTCHECK COMPLETE. SYSTEM READY.");
                        Console.WriteLine("");
                    } catch (Exception ex) {
                        Console.WriteLine("");
                        Console.WriteLine("Service account key files are invalid. They may have been tampered with.");
                        Logger.Log("BOOTCHECK - Service account key files invalid. ERROR: " + ex.Message);
                        Environment.Exit(1);
                    }
                }
            }
        }
    }
}