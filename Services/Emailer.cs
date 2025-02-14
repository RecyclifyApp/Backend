using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Backend.Services {
    public class Emailer {
        private readonly MyDbContext _context;
        public Emailer(MyDbContext context) {
            _context = context;
        }

        private static readonly string? SenderEmail = Environment.GetEnvironmentVariable("EMAIL_SENDER");

        private static readonly string? EmailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

        private async Task CheckPermissionAsync() {
            var emailerEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "EMAILER_ENABLED");
            if (emailerEnabled == null) {
                throw new InvalidOperationException("ERROR: EMAILER_ENABLED configuration is missing.");
            }
            if (emailerEnabled.Value == "false") {
                throw new Exception("ERROR: Emailer Service is Disabled.");
            }
        }

        public async Task<string> SendEmailAsync(string to, string subject, string template, Dictionary<string, string> variables) {
            await CheckPermissionAsync();

            try {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Recyclify System", SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var templatePath = "templates/emails/" + template + ".html";

                if (!File.Exists(templatePath)) {
                    return $"ERROR: Template file '{template}' not found.";
                }

                string htmlTemplate = File.ReadAllText(templatePath);

                foreach (var variable in variables) {
                    htmlTemplate = htmlTemplate.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                }

                var builder = new BodyBuilder {
                    HtmlBody = htmlTemplate
                };

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(SenderEmail, EmailPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return "SUCCESS: Email sent successfully.";
            } catch (Exception ex) {
                return $"ERROR: Failed to send email to {to}. Error: {ex.Message}";
            }
        }
    }
}