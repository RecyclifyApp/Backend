using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class WeeklyClassPoints {
        [Key, Column(Order = 0)]
        public required DateTime Date { get; set; }

        public required int PointsGained { get; set; } = 0;

        [Key, Column(Order = 1)]
        public required string ClassID { get; set; }
        public required Class Class { get; set; }
    }
}