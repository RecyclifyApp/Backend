using MailKit.Net.Smtp;
using MimeKit;

namespace Backend.Services {
    public static class Emailer {
        private static bool ContextChecked = false;
        private static readonly string? SenderEmail = Environment.GetEnvironmentVariable("EMAIL_SENDER");

        public static bool CheckPermission() {
            return Environment.GetEnvironmentVariable("EMAILING_ENABLED") == "True";
        }

        public static void CheckContext() {
            if (CheckPermission()) {
                var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                if (string.IsNullOrEmpty(SenderEmail) || string.IsNullOrEmpty(emailPassword)) {
                    throw new Exception("ERROR: EMAIL_ADDRESS or EMAIL_PASSWORD environment variables not set.");
                }
            }

            ContextChecked = true;
        }

        public static async Task<string> SendEmailAsync(string to, string subject, string template) {
            if (!ContextChecked) {
                return "ERROR: System context was not checked before sending email. Skipping email.";
            }

            if (!CheckPermission()) {
                return "ERROR: Emailing services are not enabled. Skipping email.";
            }

            try {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Recyclify System", SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var templatePath = "../Backend/templates/emails/" + template + ".html";

                if (!File.Exists(templatePath)) {
                    return $"ERROR: Template file '{template}' not found.";
                }

                string htmlTemplate = File.ReadAllText(templatePath);

                var builder = new BodyBuilder {
                    HtmlBody = htmlTemplate
                };

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(SenderEmail, Environment.GetEnvironmentVariable("EMAIL_PASSWORD"));
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return "SUCCESS: Email sent successfully.";
            } catch (Exception ex) {
                return $"ERROR: Failed to send email to {to}. Error: {ex.Message}";
            }
        }
    }
}