using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CloudDBV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    QuestID = table.Column<string>(type: "varchar(255)", nullable: false),
                    QuestTitle = table.Column<string>(type: "longtext", nullable: false),
                    QuestDescription = table.Column<string>(type: "longtext", nullable: false),
                    QuestPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.QuestID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RewardItems",
                columns: table => new
                {
                    RewardID = table.Column<string>(type: "varchar(255)", nullable: false),
                    RewardTitle = table.Column<string>(type: "longtext", nullable: false),
                    RewardDescription = table.Column<string>(type: "longtext", nullable: false),
                    RequiredPoints = table.Column<int>(type: "int", nullable: false),
                    RewardQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardItems", x => x.RewardID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    TaskID = table.Column<string>(type: "varchar(255)", nullable: false),
                    TaskTitle = table.Column<string>(type: "longtext", nullable: false),
                    TaskDescription = table.Column<string>(type: "longtext", nullable: false),
                    TaskPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.TaskID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: false),
                    Password = table.Column<string>(type: "longtext", nullable: false),
                    ContactNumber = table.Column<string>(type: "longtext", nullable: true),
                    UserRole = table.Column<string>(type: "longtext", nullable: false),
                    Avatar = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminID = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AdminID);
                    table.ForeignKey(
                        name: "FK_Admins_Users_AdminID",
                        column: x => x.AdminID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Inboxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(type: "longtext", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserID = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inboxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inboxes_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    TeacherID = table.Column<string>(type: "varchar(255)", nullable: false),
                    TeacherName = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.TeacherID);
                    table.ForeignKey(
                        name: "FK_Teachers_Users_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    ClassID = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClassName = table.Column<string>(type: "longtext", nullable: false),
                    ClassPoints = table.Column<int>(type: "int", nullable: false),
                    TeacherID = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.ClassID);
                    table.ForeignKey(
                        name: "FK_Classes_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuestProgresses",
                columns: table => new
                {
                    QuestID = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClassID = table.Column<string>(type: "varchar(255)", nullable: false),
                    Progress = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestProgresses", x => x.QuestID);
                    table.ForeignKey(
                        name: "FK_QuestProgresses_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestProgresses_Quests_QuestID",
                        column: x => x.QuestID,
                        principalTable: "Quests",
                        principalColumn: "QuestID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClassID = table.Column<string>(type: "varchar(255)", nullable: false),
                    ParentID = table.Column<string>(type: "longtext", nullable: false),
                    CurrentPoints = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentID);
                    table.ForeignKey(
                        name: "FK_Students_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Students_Users_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WeeklyClassPoints",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ClassID = table.Column<string>(type: "varchar(255)", nullable: false),
                    PointsGained = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyClassPoints", x => new { x.Date, x.ClassID });
                    table.ForeignKey(
                        name: "FK_WeeklyClassPoints_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DailyStudentPoints",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "varchar(255)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PointsGained = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyStudentPoints", x => new { x.StudentID, x.Date });
                    table.ForeignKey(
                        name: "FK_DailyStudentPoints_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Parents",
                columns: table => new
                {
                    ParentID = table.Column<string>(type: "varchar(255)", nullable: false),
                    StudentID = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parents", x => x.ParentID);
                    table.ForeignKey(
                        name: "FK_Parents_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parents_Users_ParentID",
                        column: x => x.ParentID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Redemptions",
                columns: table => new
                {
                    RedemptionID = table.Column<string>(type: "varchar(255)", nullable: false),
                    RedeemedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClaimedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RedemptionStatus = table.Column<string>(type: "longtext", nullable: false),
                    RewardID = table.Column<string>(type: "varchar(255)", nullable: false),
                    StudentID = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redemptions", x => x.RedemptionID);
                    table.ForeignKey(
                        name: "FK_Redemptions_RewardItems_RewardID",
                        column: x => x.RewardID,
                        principalTable: "RewardItems",
                        principalColumn: "RewardID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Redemptions_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaskProgresses",
                columns: table => new
                {
                    TaskID = table.Column<string>(type: "varchar(255)", nullable: false),
                    StudentID = table.Column<string>(type: "varchar(255)", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: true),
                    TaskVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AssignedTeacherID = table.Column<string>(type: "varchar(255)", nullable: false),
                    ImageUrls = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskProgresses", x => x.TaskID);
                    table.ForeignKey(
                        name: "FK_TaskProgresses_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskProgresses_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Tasks",
                        principalColumn: "TaskID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskProgresses_Teachers_AssignedTeacherID",
                        column: x => x.AssignedTeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherID",
                table: "Classes",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_UserID",
                table: "Inboxes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Parents_StudentID",
                table: "Parents",
                column: "StudentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestProgresses_ClassID",
                table: "QuestProgresses",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_RewardID",
                table: "Redemptions",
                column: "RewardID");

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_StudentID",
                table: "Redemptions",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassID",
                table: "Students",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskProgresses_AssignedTeacherID",
                table: "TaskProgresses",
                column: "AssignedTeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskProgresses_StudentID",
                table: "TaskProgresses",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyClassPoints_ClassID",
                table: "WeeklyClassPoints",
                column: "ClassID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "DailyStudentPoints");

            migrationBuilder.DropTable(
                name: "Inboxes");

            migrationBuilder.DropTable(
                name: "Parents");

            migrationBuilder.DropTable(
                name: "QuestProgresses");

            migrationBuilder.DropTable(
                name: "Redemptions");

            migrationBuilder.DropTable(
                name: "TaskProgresses");

            migrationBuilder.DropTable(
                name: "WeeklyClassPoints");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropTable(
                name: "RewardItems");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
