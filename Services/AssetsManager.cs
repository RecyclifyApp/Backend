using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace Backend.Services {
    public static class AssetsManager {
        private static readonly StorageClient _storageClient;

        static AssetsManager() {
            var credentialsPath = Environment.GetEnvironmentVariable("FIREBASE_APPLICATION_CREDENTIALS");

            if (string.IsNullOrEmpty(credentialsPath)) {
                throw new Exception("ERROR: FIREBASE_APPLICATION_CREDENTIALS environment variable not set.");
            }

            FirebaseApp.Create(new AppOptions() {
                Credential = GoogleCredential.FromFile(credentialsPath)
            });

            _storageClient = StorageClient.Create(GoogleCredential.FromFile(credentialsPath));
        }

        public static async Task<string> UploadFileAsync(Stream stream, string fileName, string contentType) {
            if (stream == null || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(contentType)) {
                return "ERROR: One or more required parameters are missing.";
            } else {
                try {
                    var bucketName = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET_URL");

                    if (string.IsNullOrEmpty(bucketName)) {
                        return "ERROR: FIREBASE_STORAGE_BUCKET_URL environment variable not set.";
                    }

                    var storageObject = await _storageClient.UploadObjectAsync(
                        bucketName,
                        fileName,
                        contentType,
                        stream
                    );

                    return $"SUCCESS: File uploaded to cloud storage.";
                } catch (Exception ex) {
                    return $"ERROR: {ex.Message}"; // Implement Logger
                }
            }
        }

        public static async Task<string> DeleteFileAsync(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                return "ERROR: Invalid file name. Please provide a valid file name.";
            } else {
                try {
                    var bucketName = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET_URL");

                    if (string.IsNullOrEmpty(bucketName)) {
                        return "ERROR: FIREBASE_STORAGE_BUCKET_URL environment variable not set.";
                    }

                    await _storageClient.DeleteObjectAsync(bucketName, fileName);

                    return $"SUCCESS: File {fileName} deleted from bucket {bucketName}";
                } catch (Exception ex) {
                    return $"ERROR: {ex.Message}"; // Implement Logger
                }
            }
        }

        public static async Task<string> GetFileUrlAsync(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                return "ERROR: Invalid file name. Please provide a valid file name.";
            } else {
                try {
                    var bucketName = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET_URL");

                    var storageObject = await _storageClient.GetObjectAsync(bucketName, fileName);

                    if (storageObject == null) {
                        return $"ERROR: File {fileName} not found in cloud storage.";
                    }

                    var fileUrl = $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{fileName}?alt=media";

                    return $"SUCCESS: File URL: {fileUrl}";
                } catch (Exception ex) {
                    return $"ERROR: {ex.Message}"; // Implement Logger
                }
            }
        }
    }
}