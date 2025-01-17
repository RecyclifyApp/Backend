using System.ComponentModel.DataAnnotations;

namespace Backend.Models {
    public class RewardItem {
        [Key]
        public required string RewardID { get; set; }
        public required string RewardTitle { get; set; }
        public required string RewardDescription { get; set; }
        public required int RequiredPoints { get; set; } = 0;
        public required int RewardQuantity { get; set; } = 0;
        public required bool IsAvailable { get; set; } = true;
    }
}