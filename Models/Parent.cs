using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class Parent {
        [Key]
        [ForeignKey(nameof(Student))]
        public required string ParentID { get; set; }

        public required string StudentID { get; set; }
        public required Student Student { get; set; }
    }
}