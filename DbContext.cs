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
        public required DbSet<ContactForm> ContactForms { get; set; }

        public MyDbContext(IConfiguration configuration) : base() {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            string? dbMode = Environment.GetEnvironmentVariable("DB_MODE");
            if (dbMode == "cloud") {
                string? connectionString = Environment.GetEnvironmentVariable("CLOUDSQL_CONNECTION_STRING");
                if (connectionString != null) {
                    optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33)));
                }
            } else if (dbMode == "local") {
                string sqlitePath = "database.sqlite";
                optionsBuilder.UseSqlite($"Data Source={sqlitePath}");
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Admin>()
                .HasKey(a => a.AdminID);

            modelBuilder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.AdminID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Parent>()
                .HasKey(p => p.ParentID);

            modelBuilder.Entity<Parent>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Parent>(p => p.ParentID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Parent>()
                .HasOne(p => p.Student)
                .WithOne(s => s.Parent)
                .HasForeignKey<Parent>(p => p.ParentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Student>(s => s.StudentID)
                .IsRequired()   
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Parent)
                .WithOne(p => p.Student)
                .HasForeignKey<Parent>(p => p.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyStudentPoints>()
                .HasKey(dsp => new { dsp.StudentID, dsp.Date });

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
                .HasOne(qp => qp.Class)
                .WithMany(c => c.QuestProgresses)
                .HasForeignKey(qp => qp.ClassID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(t => t.Task)
                .WithMany()
                .HasForeignKey(t => t.TaskID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(t => t.AssignedTeacher)
                .WithMany()
                .HasForeignKey(t => t.AssignedTeacherID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskProgress>()
                .Property(t => t.ImageUrls)
                .HasColumnType("text");

            modelBuilder.Entity<WeeklyClassPoints>()
                .HasKey(w => new { w.Date, w.ClassID });

            modelBuilder.Entity<WeeklyClassPoints>()
                .HasOne(w => w.Class)
                .WithMany(c => c.WeeklyClassPoints)
                .HasForeignKey(w => w.ClassID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Redemption>()
                .HasOne(r => r.Student)
                .WithMany(s => s.Redemptions)
                .HasForeignKey(r => r.StudentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Teacher>()
                .HasKey(t => t.TeacherID);

            modelBuilder.Entity<Teacher>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Teacher>(t => t.TeacherID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.TeacherName)
                .HasPrincipalKey(u => u.Name)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Teacher>()
                .HasMany(t => t.Classes)
                .WithOne(c => c.Teacher)
                .HasForeignKey(c => c.TeacherID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Class>()
                .HasKey(c => c.ClassID);

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Classes)
                .HasForeignKey(c => c.TeacherID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}