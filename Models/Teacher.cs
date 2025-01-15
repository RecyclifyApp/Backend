using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class Teacher {
        public required string TeacherID { get; set; }
        public required string TeacherName { get; set; }
        public string? UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }
        public ICollection<Class>? Classes { get; set; }
    }
}