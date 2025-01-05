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

        public static async Task<string> UploadFileAsync(IFormFile file) {
            if (file == null || file.Length == 0) {
                return "ERROR: One or more required parameters are missing.";
            } else {
                try {
                    var bucketName = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET_URL");

                    if (string.IsNullOrEmpty(bucketName)) {
                        return "ERROR: FIREBASE_STORAGE_BUCKET_URL environment variable not set.";
                    }

                    var stream = file.OpenReadStream();
                    var fileName = file.FileName;
                    var contentType = file.ContentType;

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
            }

            var bucketName = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET_URL");

            if (string.IsNullOrEmpty(bucketName)) {
                return "ERROR: FIREBASE_STORAGE_BUCKET_URL environment variable not set.";
            }

            try {
                try {
                    var obj = await _storageClient.GetObjectAsync(bucketName, fileName);
                    if (obj == null) {
                        return $"ERROR: File {fileName} does not exist.";
                    }
                } catch (Google.GoogleApiException ex) when (ex.Error.Code == 404) {
                    return $"ERROR: File {fileName} does not exist.";
                }

                await _storageClient.DeleteObjectAsync(bucketName, fileName);
                return $"SUCCESS: File {fileName} deleted from bucket {bucketName}.";
            } catch (Exception ex) {
                Logger.Log($"ASSETSMANAGER ERROR: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }


        public static async Task<string> GetFileUrlAsync(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                return "ERROR: Invalid file name. Please provide a valid file name.";
            }

            var bucketName = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET_URL");

            if (string.IsNullOrEmpty(bucketName)) {
                return "ERROR: FIREBASE_STORAGE_BUCKET_URL environment variable not set.";
            }

            try {
                try {
                    var storageObject = await _storageClient.GetObjectAsync(bucketName, fileName);
                    if (storageObject == null) {
                        return $"ERROR: File {fileName} does not exist.";
                    }
                } catch (Google.GoogleApiException ex) when (ex.Error.Code == 404) {
                    return $"ERROR: File {fileName} does not exist.";
                }

                var fileUrl = $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{fileName}?alt=media";
                return $"SUCCESS: {fileUrl}";
            } catch (Exception ex) {
                Logger.Log($"ASSETSMANAGER ERROR: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }
    }
}