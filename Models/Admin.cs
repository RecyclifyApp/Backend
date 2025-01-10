using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class Admin {
        [Key]
        [ForeignKey(nameof(User))]
        public required string AdminID { get; set; }

        public required User User { get; set; }
    }
}
