using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json.Serialization;

namespace Backend.Models {
    public class TaskProgress {
        [Key, Column(Order = 1)]
        public required string TaskID { get; set; }

        [ForeignKey("TaskID")]
        public required Task Task { get; set; }

        [Key, Column(Order = 2)]
        public required string StudentID { get; set; }

        [Key, Column(Order = 3)]
        public required string DateAssigned { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentID")]
        public Student? Student { get; set; }

        public required bool TaskVerified { get; set; } = false;
        public required bool TaskRejected { get; set; } = false;
        public required bool VerificationPending { get; set; } = false;

        [ForeignKey("AssignedTeacherID")]
        public required string AssignedTeacherID { get; set; }

        public required virtual Teacher AssignedTeacher { get; set; }

        public string? ImageUrls { get; set; }
    }
} 
