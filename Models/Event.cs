
namespace Backend.Models {
    public class Event {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string EventDateTime { get; set; }
        public required string PostedDateTime { get; set; }
        public required string ImageUrl { get; set; }

    }
}
