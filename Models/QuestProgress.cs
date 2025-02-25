using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models {
    public class QuestProgress {
        [Key, Column(Order = 1)]
        public required string QuestID { get; set; }

        [Key, Column(Order = 2)]
        public required string ClassID { get; set; }

        [Key, Column(Order = 3)]
        public required string DateAssigned { get; set; }
        public required int AmountCompleted { get; set; } = 0;
        public required bool Completed { get; set; } = false;

        [ForeignKey("QuestID")]
        public required Quest Quest { get; set; }

        [JsonIgnore]
        [ForeignKey("ClassID")]
        public Class? Class { get; set; }

        [ForeignKey("AssignedTeacherID")]
        public required string AssignedTeacherID { get; set; }

        public required virtual Teacher AssignedTeacher { get; set; }
    }
}
