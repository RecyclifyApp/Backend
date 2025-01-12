using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services {
    public static class Utilities {
        public static void SaveToSqlite(MyDbContext context) { // Use this method to save data to local SQLite database, instead of traditionally just using context.SaveChanges()
            context.SaveChanges();
            context.Database.ExecuteSqlInterpolated($"PRAGMA wal_checkpoint(FULL)");
            context.Dispose();
            RemoveTempFiles();
        }

        private static void RemoveTempFiles() {
            if (File.Exists("database.sqlite-shm")) {
                File.Delete("database.sqlite-shm");
            }
            if (File.Exists("database.sqlite-wal")) {
                File.Delete("database.sqlite-wal");
            }
        }

        public static string GenerateUniqueID(int customLength = 0) {
            if (customLength == 0) {
                return Guid.NewGuid().ToString();
            } else {
                var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var random = new Random();
                var builder = new StringBuilder(customLength);

                for (int i = 0; i < customLength; i++) {
                    builder.Append(characters[random.Next(characters.Length)]);
                }

                return builder.ToString();
            }
        }

        public static string HashString(string input) {
            if (string.IsNullOrEmpty(input)) {
                throw new ArgumentException("Invalid input. Please provide a valid string to hash.");
            } else {
                using (var sha256 = SHA256.Create()) {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                    return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                }
            }
        }

        public static string EncodeToBase64(string input) {
            if (string.IsNullOrEmpty(input)) {
                throw new ArgumentException("Invalid input. Please provide a valid string to encode.");
            } else {
                var bytes = Encoding.UTF8.GetBytes(input);
                return Convert.ToBase64String(bytes);
            }
        }

        public static string DecodeFromBase64(string input) {
            if (string.IsNullOrEmpty(input)) {
                throw new ArgumentException("Invalid input. Please provide a valid string to decode.");
            } else {
                var bytes = Convert.FromBase64String(input);
                return Encoding.UTF8.GetString(bytes);
            }
        }
    }
}