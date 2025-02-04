using System.ComponentModel.DataAnnotations;

namespace Backend.Models {
    public class ClassPoints {
        [Key]
        public required string ClassID { get; set; }
        public required string QuestID { get; set; }
        public required string ContributingStudentID { get; set; }
        public required string DateCompleted { get; set; }
        public required int PointsAwarded { get; set; }
    }
}