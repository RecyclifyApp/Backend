using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend {
    public class MyDbContext : DbContext {
        private readonly IConfiguration _configuration;

        public required DbSet<Teacher> Teachers { get; set; }
        public required DbSet<Class> Classes { get; set; }
        public required DbSet<Student> Students { get; set; }
        public required DbSet<Parent> Parents { get; set; }
        public required DbSet<DailyStudentPoints> DailyStudentPoints { get; set; }
        public required DbSet<Quest> Quests { get; set; }
        public required DbSet<QuestProgress> QuestProgresses { get; set; }
        public required DbSet<Models.Task> Tasks { get; set; }
        public required DbSet<TaskProgress> TaskProgresses { get; set; }
        public required DbSet<RewardItem> RewardItems { get; set; }
        public required DbSet<Redemption> Redemptions { get; set; }
        public required DbSet<Inbox> Inboxes { get; set; }
        public required DbSet<Admin> Admins { get; set; }
        public required DbSet<User> Users { get; set; }
        public required DbSet<WeeklyClassPoints> WeeklyClassPoints { get; set; }

        public MyDbContext(IConfiguration configuration) : base() {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            string? connectionString = Environment.GetEnvironmentVariable("CLOUDSQL_CONNECTION_STRING");
            if (connectionString != null) {
                optionsBuilder.UseMySQL(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Parent)
                .WithOne(p => p.Student)
                .HasForeignKey<Student>(s => s.ParentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyStudentPoints>()
                .HasOne(d => d.Student)
                .WithMany()
                .HasForeignKey(d => d.StudentID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inbox>()
                .HasOne(i => i.User)
                .WithMany(u => u.Inboxes)
                .HasForeignKey(i => i.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuestProgress>()
                .HasOne(qp => qp.Quest)
                .WithMany()
                .HasForeignKey(qp => qp.QuestID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuestProgress>()
                .HasOne(q => q.Class)
                .WithMany(c => c.QuestProgresses)
                .HasForeignKey(q => q.ClassID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(t => t.Task)
                .WithMany()
                .HasForeignKey(t => t.TaskID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WeeklyClassPoints>()
                .HasKey(w => new { w.Date, w.ClassID });

            modelBuilder.Entity<WeeklyClassPoints>()
                .HasOne(w => w.Class)
                .WithMany(c => c.WeeklyClassPoints)
                .HasForeignKey(w => w.ClassID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}