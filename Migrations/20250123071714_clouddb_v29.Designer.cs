﻿// <auto-generated />
using System;
using Backend;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Backend.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20250123071714_clouddb_v29")]
    partial class clouddb_v29
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Backend.Models.Admin", b =>
                {
                    b.Property<string>("AdminID")
                        .HasColumnType("varchar(255)");

                    b.HasKey("AdminID");

                    b.ToTable("Admins");
                });

            modelBuilder.Entity("Backend.Models.Class", b =>
                {
                    b.Property<string>("ClassID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ClassDescription")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ClassImage")
                        .HasColumnType("longtext");

                    b.Property<int>("ClassName")
                        .HasColumnType("int");

                    b.Property<int>("ClassPoints")
                        .HasColumnType("int");

                    b.Property<string>("TeacherID")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("ClassID");

                    b.HasIndex("TeacherID");

                    b.ToTable("Classes");
                });

            modelBuilder.Entity("Backend.Models.ClassPoints", b =>
                {
                    b.Property<string>("ClassID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("QuestID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("DateCompleted")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("PointsAwarded")
                        .HasColumnType("int");

                    b.HasKey("ClassID", "QuestID", "DateCompleted");

                    b.ToTable("ClassPoints");
                });

            modelBuilder.Entity("Backend.Models.ClassStudents", b =>
                {
                    b.Property<string>("ClassID")
                        .HasColumnType("varchar(255)")
                        .HasColumnOrder(0);

                    b.Property<string>("StudentID")
                        .HasColumnType("varchar(255)")
                        .HasColumnOrder(1);

                    b.HasKey("ClassID", "StudentID");

                    b.HasIndex("StudentID");

                    b.ToTable("ClassStudents");
                });

            modelBuilder.Entity("Backend.Models.ContactForm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("HasReplied")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("SenderEmail")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("SenderName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("ContactForms");
                });

            modelBuilder.Entity("Backend.Models.DailyStudentPoints", b =>
                {
                    b.Property<string>("StudentID")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("PointsGained")
                        .HasColumnType("int");

                    b.HasKey("StudentID", "Date");

                    b.ToTable("DailyStudentPoints");
                });

            modelBuilder.Entity("Backend.Models.Inbox", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserID")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserID");

                    b.ToTable("Inboxes");
                });

            modelBuilder.Entity("Backend.Models.Parent", b =>
                {
                    b.Property<string>("ParentID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("StudentID")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("UserID")
                        .HasColumnType("varchar(255)");

                    b.HasKey("ParentID");

                    b.HasIndex("StudentID")
                        .IsUnique();

                    b.HasIndex("UserID");

                    b.ToTable("Parents");
                });

            modelBuilder.Entity("Backend.Models.Quest", b =>
                {
                    b.Property<string>("QuestID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("QuestDescription")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("QuestPoints")
                        .HasColumnType("int");

                    b.Property<string>("QuestTitle")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("QuestID");

                    b.ToTable("Quests");
                });

            modelBuilder.Entity("Backend.Models.QuestProgress", b =>
                {
                    b.Property<string>("QuestID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ClassID")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Progress")
                        .HasColumnType("longtext");

                    b.HasKey("QuestID");

                    b.HasIndex("ClassID");

                    b.ToTable("QuestProgresses");
                });

            modelBuilder.Entity("Backend.Models.Redemption", b =>
                {
                    b.Property<string>("RedemptionID")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime?>("ClaimedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("RedeemedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("RedemptionStatus")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RewardID")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("StudentID")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("RedemptionID");

                    b.HasIndex("RewardID");

                    b.HasIndex("StudentID");

                    b.ToTable("Redemptions");
                });

            modelBuilder.Entity("Backend.Models.RewardItem", b =>
                {
                    b.Property<string>("RewardID")
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("RequiredPoints")
                        .HasColumnType("int");

                    b.Property<string>("RewardDescription")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("RewardQuantity")
                        .HasColumnType("int");

                    b.Property<string>("RewardTitle")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("RewardID");

                    b.ToTable("RewardItems");
                });

            modelBuilder.Entity("Backend.Models.Student", b =>
                {
                    b.Property<string>("StudentID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ClassID")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("CurrentPoints")
                        .HasColumnType("int");

                    b.Property<string>("LastClaimedStreak")
                        .HasColumnType("longtext");

                    b.Property<string>("League")
                        .HasColumnType("longtext");

                    b.Property<int?>("LeagueRank")
                        .HasColumnType("int");

                    b.Property<string>("ParentID")
                        .HasColumnType("longtext");

                    b.Property<int>("Streak")
                        .HasColumnType("int");

                    b.Property<DateTime?>("TaskLastSet")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("TotalPoints")
                        .HasColumnType("int");

                    b.Property<string>("UserID")
                        .HasColumnType("varchar(255)");

                    b.HasKey("StudentID");

                    b.HasIndex("ClassID");

                    b.HasIndex("UserID");

                    b.ToTable("Students");
                });

            modelBuilder.Entity("Backend.Models.StudentPoints", b =>
                {
                    b.Property<string>("StudentID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("TaskID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("DateCompleted")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("PointsAwarded")
                        .HasColumnType("int");

                    b.HasKey("StudentID", "TaskID", "DateCompleted");

                    b.ToTable("StudentPoints");
                });

            modelBuilder.Entity("Backend.Models.Task", b =>
                {
                    b.Property<string>("TaskID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("TaskDescription")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("TaskPoints")
                        .HasColumnType("int");

                    b.Property<string>("TaskTitle")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("TaskID");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("Backend.Models.TaskProgress", b =>
                {
                    b.Property<string>("TaskID")
                        .HasColumnType("varchar(255)")
                        .HasColumnOrder(1);

                    b.Property<string>("StudentID")
                        .HasColumnType("varchar(255)")
                        .HasColumnOrder(2);

                    b.Property<string>("DateAssigned")
                        .HasColumnType("varchar(255)")
                        .HasColumnOrder(3);

                    b.Property<string>("AssignedTeacherID")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ImageUrls")
                        .HasColumnType("text");

                    b.Property<bool>("TaskVerified")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("VerificationPending")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("TaskID", "StudentID", "DateAssigned");

                    b.HasIndex("AssignedTeacherID");

                    b.HasIndex("StudentID");

                    b.ToTable("TaskProgresses");
                });

            modelBuilder.Entity("Backend.Models.Teacher", b =>
                {
                    b.Property<string>("TeacherID")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("TeacherName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserID")
                        .HasColumnType("varchar(255)");

                    b.HasKey("TeacherID");

                    b.HasIndex("UserID");

                    b.ToTable("Teachers");
                });

            modelBuilder.Entity("Backend.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Avatar")
                        .HasColumnType("longtext");

                    b.Property<string>("ContactNumber")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("EmailVerificationToken")
                        .HasColumnType("longtext");

                    b.Property<string>("EmailVerificationTokenExpiry")
                        .HasColumnType("longtext");

                    b.Property<bool>("EmailVerified")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("FName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("LName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserRole")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Backend.Models.WeeklyClassPoints", b =>
                {
                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)")
                        .HasColumnOrder(0);

                    b.Property<string>("ClassID")
                        .HasColumnType("varchar(255)")
                        .HasColumnOrder(1);

                    b.Property<int>("PointsGained")
                        .HasColumnType("int");

                    b.HasKey("Date", "ClassID");

                    b.HasIndex("ClassID");

                    b.ToTable("WeeklyClassPoints");
                });

            modelBuilder.Entity("Backend.Models.Admin", b =>
                {
                    b.HasOne("Backend.Models.User", "User")
                        .WithOne("Admin")
                        .HasForeignKey("Backend.Models.Admin", "AdminID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Backend.Models.Class", b =>
                {
                    b.HasOne("Backend.Models.Teacher", "Teacher")
                        .WithMany("Classes")
                        .HasForeignKey("TeacherID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("Backend.Models.ClassStudents", b =>
                {
                    b.HasOne("Backend.Models.Class", "Class")
                        .WithMany()
                        .HasForeignKey("ClassID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.Student", "Student")
                        .WithMany()
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Backend.Models.DailyStudentPoints", b =>
                {
                    b.HasOne("Backend.Models.Student", "Student")
                        .WithMany()
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Backend.Models.Inbox", b =>
                {
                    b.HasOne("Backend.Models.User", "User")
                        .WithMany("Inboxes")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Backend.Models.Parent", b =>
                {
                    b.HasOne("Backend.Models.User", null)
                        .WithOne()
                        .HasForeignKey("Backend.Models.Parent", "ParentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.Student", "Student")
                        .WithOne("Parent")
                        .HasForeignKey("Backend.Models.Parent", "StudentID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Backend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID");

                    b.Navigation("Student");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Backend.Models.QuestProgress", b =>
                {
                    b.HasOne("Backend.Models.Class", "Class")
                        .WithMany("QuestProgresses")
                        .HasForeignKey("ClassID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.Quest", "Quest")
                        .WithMany()
                        .HasForeignKey("QuestID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("Quest");
                });

            modelBuilder.Entity("Backend.Models.Redemption", b =>
                {
                    b.HasOne("Backend.Models.RewardItem", "Reward")
                        .WithMany()
                        .HasForeignKey("RewardID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.Student", "Student")
                        .WithMany("Redemptions")
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Reward");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Backend.Models.Student", b =>
                {
                    b.HasOne("Backend.Models.Class", null)
                        .WithMany("Students")
                        .HasForeignKey("ClassID");

                    b.HasOne("Backend.Models.User", null)
                        .WithOne()
                        .HasForeignKey("Backend.Models.Student", "StudentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Backend.Models.TaskProgress", b =>
                {
                    b.HasOne("Backend.Models.Teacher", "AssignedTeacher")
                        .WithMany()
                        .HasForeignKey("AssignedTeacherID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.Student", "Student")
                        .WithMany("TaskProgresses")
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.Task", "Task")
                        .WithMany()
                        .HasForeignKey("TaskID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AssignedTeacher");

                    b.Navigation("Student");

                    b.Navigation("Task");
                });

            modelBuilder.Entity("Backend.Models.Teacher", b =>
                {
                    b.HasOne("Backend.Models.User", null)
                        .WithOne()
                        .HasForeignKey("Backend.Models.Teacher", "TeacherID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Backend.Models.WeeklyClassPoints", b =>
                {
                    b.HasOne("Backend.Models.Class", "Class")
                        .WithMany("WeeklyClassPoints")
                        .HasForeignKey("ClassID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");
                });

            modelBuilder.Entity("Backend.Models.Class", b =>
                {
                    b.Navigation("QuestProgresses");

                    b.Navigation("Students");

                    b.Navigation("WeeklyClassPoints");
                });

            modelBuilder.Entity("Backend.Models.Student", b =>
                {
                    b.Navigation("Parent");

                    b.Navigation("Redemptions");

                    b.Navigation("TaskProgresses");
                });

            modelBuilder.Entity("Backend.Models.Teacher", b =>
                {
                    b.Navigation("Classes");
                });

            modelBuilder.Entity("Backend.Models.User", b =>
                {
                    b.Navigation("Admin");

                    b.Navigation("Inboxes");
                });
#pragma warning restore 612, 618
        }
    }
}
