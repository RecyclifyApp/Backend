using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class Redemption {
        [Key]
        public required string RedemptionID { get; set; }
        public DateTime? RedeemedOn { get; set; }
        public DateTime? ClaimedOn { get; set; }
        public required string RedemptionStatus { get; set; } = "Pending";

        public required string RewardID { get; set; }
        
        [ForeignKey(nameof(RewardID))]
        public virtual RewardItem? Reward { get; set; }
        
        public required string StudentID { get; set; }
        
        [ForeignKey(nameof(StudentID))]
        public virtual Student? Student { get; set; }
    }
}