// using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend {
    public class MyDbContext(IConfiguration configuration) : DbContext {
        private readonly IConfiguration _configuration = configuration;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            string? connectionString = Environment.GetEnvironmentVariable("CLOUDSQL_CONNECTION_STRING");
            if (connectionString != null) {
                optionsBuilder.UseMySQL(connectionString);
            }
        }

        // Use dbset to create tables here
        
    }
}