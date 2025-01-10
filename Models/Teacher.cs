namespace Backend.Models {
    public class Teacher {
        public required string TeacherID { get; set; }
        public required string Name { get; set; }
        public ICollection<Class>? Classes { get; set; }
    }
}