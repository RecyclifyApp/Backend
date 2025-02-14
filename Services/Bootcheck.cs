// This is a Bootcheck service that checks if all the necessary environment variables are set and if the service account key file is valid.
// System will stop if bootcheck fails.

using Google.Apis.Auth.OAuth2;
using Backend.Models;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;

namespace Backend.Services {
    public static class Bootcheck {
        public static async Task Run(MyDbContext context) {
            var missingVariables = new List<string>();
            var environmentVariables = new string[] {
                "DB_MODE",
                "EMAILER_ENABLED",
                "SMS_ENABLED",
                "COMPVISION_ENABLED",
                "OPENAI_CHAT_SERVICE_ENABLED",
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
                "CLOUDSQL_CONNECTION_STRING",
                "JWT_KEY",
                "OPENAI_API_KEY",
                "SUPERUSER_USERNAME",
                "SUPERUSER_PASSWORD",
                "SUPERUSER_PIN",
                "SYSTEM_LOCKED",
                "ACCREDIBLE_API_KEY",
                "ACCREDIBLE_RECYCLIFY_CERTIFICATE_GROUP_ID"
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

                        await SaveEnvironmentVariables(context, environmentVariables);

                        Console.WriteLine("");
                        Console.WriteLine("BOOTCHECK COMPLETE: CLOUD CONFIGS READY. SYSTEM STARTING...");
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

        private static async Task SaveEnvironmentVariables(MyDbContext context, string[] environmentVariables) {
            try {
                context.EnvironmentConfigs.RemoveRange(context.EnvironmentConfigs);

                foreach (var envVar in environmentVariables) {
                    var value = Environment.GetEnvironmentVariable(envVar) ?? "Not set";
                    var config = new EnvironmentConfig {
                        Name = envVar,
                        Value = value
                    };
                    
                    context.EnvironmentConfigs.Add(config);
                }
                await context.SaveChangesAsync();
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine("ERROR: Failed to save environment variables to database.");
                Logger.Log("BOOTCHECK - Failed to save environment variables to database. ERROR: " + ex.Message);
                Environment.Exit(1);
            }
        }
    }
}