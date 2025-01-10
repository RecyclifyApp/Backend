using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class Inbox {
        public int Id { get; set; }
        public required string Message { get; set; }
        public DateTime Date { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public required string UserID { get; set; }

        public required virtual User User { get; set; }
    }
}
