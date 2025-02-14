using System.ComponentModel.DataAnnotations;

namespace Backend.Models {
    public class EnvironmentConfig {
        [Key]
        public required string Name { get; set; }
        public required string Value { get; set; }
    }
}