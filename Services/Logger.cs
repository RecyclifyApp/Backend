namespace Backend.Services {
    public static class Logger {
        private static readonly string _logFilePath = Path.Combine(Environment.CurrentDirectory, "logs.txt");

        public static void Log(string message) {
            if (string.IsNullOrEmpty(message)) {
                throw new ArgumentException("Invalid message. Please provide a valid message.");
            } else {
                try {
                    using (var writer = new StreamWriter(_logFilePath, append: true)) {
                        string formattedDate = DateTime.Now.ToString("dd MMM yyyy, h:mm tt");
                        writer.WriteLine($"{formattedDate} - {message}");
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"ERROR: {ex.Message}");
                }
            }
        }
    }
}