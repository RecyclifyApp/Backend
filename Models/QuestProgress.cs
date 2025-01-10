using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class QuestProgress {
        [Key]
        public required string QuestID { get; set; }

        [ForeignKey("QuestID")]
        public required Quest Quest { get; set; }

        public required string ClassID { get; set; }

        [ForeignKey("ClassID")]
        public required Class Class { get; set; }

        public string? Progress { get; set; }
    }
}
