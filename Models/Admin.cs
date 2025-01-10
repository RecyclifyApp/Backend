namespace Backend.Models {
    public class Admin {
        public required string AdminID { get; set; }

        public required string UserID { get; set; }
        public required User User { get; set; }
    }
}