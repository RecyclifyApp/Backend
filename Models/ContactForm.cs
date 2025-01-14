using System.ComponentModel.DataAnnotations;

namespace Backend.Models {
    public class ContactForm {
        [Key]
        public required int Id { get; set; }
        public required string SenderName { get; set; }
        public required string SenderEmail { get; set; }
        public required string Message { get; set; }
    }
}