using System.ComponentModel.DataAnnotations;

namespace Backend.Models {
    public class Class {
        [Key]
        public required string ClassID { get; set; }
        public required string ClassName { get; set; }
        public required int ClassPoints { get; set; } = 0;
        public required string TeacherID { get; set; }
        public required Teacher Teacher { get; set; }
        public ICollection<Student>? Students { get; set; }
        public ICollection<QuestProgress>? QuestProgresses { get; set; }
        public required ICollection<WeeklyClassPoints> WeeklyClassPoints { get; set; } = new List<WeeklyClassPoints>();
    }
}