using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class ClassStudents {
        [Key, Column(Order = 0)]
        [ForeignKey(nameof(Class))]
        public required string ClassID { get; set; }
        public virtual Class? Class { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey(nameof(Student))]
        public required string StudentID { get; set; }
        public virtual Student? Student { get; set; }
    }
}
