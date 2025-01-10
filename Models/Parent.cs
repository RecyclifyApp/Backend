namespace Backend.Models {
    public class Parent {
        public required string ParentID { get; set; }

        // Relationships
        public required string StudentID { get; set; }
        public required Student Student { get; set; }
    }
}