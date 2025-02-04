using System.ComponentModel.DataAnnotations;

namespace Backend.Models {
    public class User {
        [Key]
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string FName { get; set; }
        public required string LName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ContactNumber { get; set; }
        public required string UserRole { get; set; }
        public string? Avatar { get; set; }
        public string? AboutMe { get; set; }
        public required bool EmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public string? EmailVerificationTokenExpiry { get; set; }
        public ICollection<Inbox>? Inboxes { get; set; }
        public Admin? Admin { get; set; }
    }
}