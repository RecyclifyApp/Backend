using System.Text;

namespace Backend.Services {
    public static class Utilities {
        // Generate a new UUID
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

        // More utilities coming soon...
    }
}