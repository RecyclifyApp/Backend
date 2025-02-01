namespace Backend.Models {
    public class Quest {
        public required string QuestID { get; set; }
        public required string QuestTitle { get; set; }
        public required string QuestDescription { get; set; }
        public required int QuestPoints { get; set; } = 0;
        public required string QuestType { get; set; }
    }
}