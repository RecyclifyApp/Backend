using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class DailyStudentPoints {
        [Key]
        [Column(Order = 0)]
        public required string StudentID { get; set; }

        public required DateTime Date { get; set; }

        public required int PointsGained { get; set; } = 0;

        [ForeignKey(nameof(StudentID))]
        public virtual Student? Student { get; set; }
    }
}
