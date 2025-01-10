using System.ComponentModel.DataAnnotations;

namespace Backend.Models {
    public class User {
        [Key]
        public required string Id { get; set; }
        public string? Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? ContactNumber { get; set; }
        public required string UserRole { get; set; }
        public string? Avatar { get; set; }

        public ICollection<Inbox>? Inboxes { get; set; }
        public Admin? Admin { get; set; }
    }
}