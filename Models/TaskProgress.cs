using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class TaskProgress {
        [Key]
        public required string TaskID { get; set; }

        [ForeignKey("TaskID")]
        public required Task Task { get; set; }

        [ForeignKey("StudentID")]
        public required string StudentID { get; set; }
        public required Student Student { get; set; }

        public int? Progress { get; set; }
        public required bool TaskVerified { get; set; } = false;
    }
}
