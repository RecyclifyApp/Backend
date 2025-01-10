using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models {
    public class Student
    {
        [Key]
        public required string StudentID { get; set; }
        
        public required string ClassID { get; set; }
        
        [ForeignKey(nameof(ClassID))]
        public virtual Class? Class { get; set; }
        
        public required string ParentID { get; set; }
        
        [ForeignKey(nameof(ParentID))]
        public virtual Parent? Parent { get; set; }
    }
}