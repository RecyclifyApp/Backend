using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int CurrentPoints { get; set; }
        public int TotalPoints { get; set; }
    }
}