using System.ComponentModel.DataAnnotations;

namespace Backend.Models {
    public class StudentPoints {
        [Key]
        public required string StudentID { get; set; }
        public required string TaskID { get; set; }
        public required string DateCompleted { get; set; }
        public required int PointsAwarded { get; set; }
    }
}