namespace Backend.Models {
    public class Task {
        public required string TaskID { get; set; }
        public required string TaskTitle { get; set; }
        public required string TaskDescription { get; set; }
        public required int TaskPoints { get; set; } = 0;
        public required int QuestContributionAmountOnComplete { get; set; }
        public required string AssociatedQuestID { get; set; }
    }
}