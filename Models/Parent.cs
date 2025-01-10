namespace Backend.Models {
    public class Parent {
        public required string ParentID { get; set; }
        public required string StudentID { get; set; }
        public required Student Student { get; set; }
    }
}