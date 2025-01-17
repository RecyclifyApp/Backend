using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DateTime = System.DateTime;

namespace Backend.Models {
    public class Student {
        [Key]
        public required string StudentID { get; set; }
        public string? ClassID { get; set; }

        [ForeignKey(nameof(ClassID))]
        public virtual Class? Class { get; set; }
        public string? ParentID { get; set; }

        [ForeignKey(nameof(ParentID))]
        public virtual Parent? Parent { get; set; }

        public ICollection<Redemption>? Redemptions { get; set; }
        public string? League { get; set; }
        public int? LeagueRank { get; set; }
        public int CurrentPoints { get; set; } = 0;
        public int TotalPoints { get; set; } = 0;   
        public string? UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        public ICollection<TaskProgress>? TaskProgresses { get; set; }
        public DateTime? TaskLastSet { get; set; }
    }
}