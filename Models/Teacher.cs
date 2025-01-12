namespace Backend.Models {
    public class Teacher {
        public required string TeacherID { get; set; }
        public required string TeacherName { get; set; }
        public User? User { get; set; }
        public ICollection<Class>? Classes { get; set; }
    }
}