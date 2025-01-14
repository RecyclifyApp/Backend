using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Task = System.Threading.Tasks.Task;

namespace Backend.Services {
    public class DatabaseManager {
        public static void SaveToSqlite(MyDbContext context) { // Use this method to save data to local SQLite database, instead of traditionally just using context.SaveChanges()
            context.SaveChangesAsync();
            context.Database.ExecuteSqlInterpolatedAsync($"PRAGMA wal_checkpoint(FULL)");
            context.DisposeAsync();
            RemoveTempFiles();
        }

        private static void RemoveTempFiles() {
            if (File.Exists("database.sqlite-shm")) {
                File.Delete("database.sqlite-shm");
            }
            if (File.Exists("database.sqlite-wal")) {
                File.Delete("database.sqlite-wal");
            }
        }

        public static void AfterUserCreate(MyDbContext context, string baseUser, List<Dictionary<string, object>> keyValuePairs) {
            var baseUserObj = new User {
                Id = keyValuePairs[0]["Id"].ToString() ?? "",
                Name = keyValuePairs[0]["Name"].ToString() ?? "",
                Email = keyValuePairs[0]["Email"].ToString() ?? "",
                Password = keyValuePairs[0]["Password"].ToString() ?? "",
                ContactNumber = keyValuePairs[0]["ContactNumber"].ToString() ?? "",
                UserRole = keyValuePairs[0]["UserRole"].ToString() ?? "",
                Avatar = keyValuePairs[0]["Avatar"].ToString() ?? ""
            };

            if (baseUser == "student") {
                var specificStudentObj = new Student {
                    StudentID = baseUserObj.Id
                };

                context.Users.Add(baseUserObj);
                context.Students.Add(specificStudentObj);
            } else if (baseUser == "admin") {
                var specificAdminObj = new Admin {
                    AdminID = baseUserObj.Id,
                    User = baseUserObj
                };

                context.Users.Add(baseUserObj);
                context.Admins.Add(specificAdminObj);
            } else if (baseUser == "teacher") {
                var specificTeacherObj = new Teacher {
                    TeacherID = baseUserObj.Id,
                    TeacherName = baseUserObj.Name
                };

                context.Users.Add(baseUserObj);
                context.Teachers.Add(specificTeacherObj);
            } else if (baseUser == "parent") {
                var studentFound = context.Students.Find(keyValuePairs[0]["StudentID"].ToString());
                if (studentFound == null) {
                    throw new ArgumentException("Invalid student ID.");
                } else {
                    var specificParentObj = new Parent {
                        ParentID = baseUserObj.Id,
                        StudentID = keyValuePairs[0]["StudentID"].ToString() ?? "",
                        Student = studentFound
                    };

                    context.Users.Add(baseUserObj);
                    context.Parents.Add(specificParentObj);
                }
            } else {
                throw new ArgumentException("Invalid user role.");
            }

            string dbMode = Environment.GetEnvironmentVariable("DB_MODE") ?? "";
            if (dbMode == "cloud") {
                context.SaveChanges();
            } else if (dbMode == "local") {
                SaveToSqlite(context);
            } else {
                throw new ArgumentException("Invalid DB_MODE configuration.");
            }
        }

        public static async Task CleanAndPopulateDatabase(MyDbContext context) {
            context.Teachers.RemoveRange(context.Teachers);
            context.Classes.RemoveRange(context.Classes);
            context.Students.RemoveRange(context.Students);
            context.Parents.RemoveRange(context.Parents);
            context.DailyStudentPoints.RemoveRange(context.DailyStudentPoints);
            context.Quests.RemoveRange(context.Quests);
            context.QuestProgresses.RemoveRange(context.QuestProgresses);
            context.Tasks.RemoveRange(context.Tasks);
            context.TaskProgresses.RemoveRange(context.TaskProgresses);
            context.RewardItems.RemoveRange(context.RewardItems);
            context.Redemptions.RemoveRange(context.Redemptions);
            context.Inboxes.RemoveRange(context.Inboxes);
            context.Admins.RemoveRange(context.Admins);
            context.Users.RemoveRange(context.Users);
            context.WeeklyClassPoints.RemoveRange(context.WeeklyClassPoints);
            context.ContactForms.RemoveRange(context.ContactForms);

            var teacher1 = new Teacher {
                TeacherID = "c1f76fc4-c99b-4517-9eac-c5ae54bb8808",
                TeacherName = "Teacher 1"
            };

            var student1 = new Student {
                StudentID = "73ecc6b8-805e-46ff-bbc3-bec52073e25d",
                ClassID = "ca4daece-c27c-46a1-99d5-1c8fd650165e",
                ParentID = "f1f76fc4-c99b-4517-9eac-c5ae54bb8808",
                CurrentPoints = 100,
                TotalPoints = 500
            };

            var student2 = new Student {
                StudentID = "3f9056b0-06e1-487a-8901-586bafd1e492",
                ClassID = "013e1876-281a-4db6-b0c8-3263ffbd0fd7",
                ParentID = "8abd80e4-05e4-4577-b6a8-edae1f419840",
                CurrentPoints = 200,
                TotalPoints = 500
            };

            var class1 = new Class {
                ClassID = "ca4daece-c27c-46a1-99d5-1c8fd650165e",
                ClassName = 101,
                ClassDescription = "Class 202 Description",
                ClassPoints = 1000,
                TeacherID = teacher1.TeacherID,
                Teacher = teacher1,
                WeeklyClassPoints = new List<WeeklyClassPoints>()
            };

            class1.WeeklyClassPoints = new List<WeeklyClassPoints> {
                new WeeklyClassPoints {
                    ClassID = "ca4daece-c27c-46a1-99d5-1c8fd650165e",
                    Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                    PointsGained = 1000,
                    Class = class1
                },
                new WeeklyClassPoints {
                    ClassID = "ca4daece-c27c-46a1-99d5-1c8fd650165e",
                    Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                    PointsGained = 2000,
                    Class = class1
                },
                new WeeklyClassPoints {
                    ClassID = "ca4daece-c27c-46a1-99d5-1c8fd650165e",
                    Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                    PointsGained = 3000,
                    Class = class1
                }
            };

            var class2 = new Class {
                ClassID = "013e1876-281a-4db6-b0c8-3263ffbd0fd7",
                ClassName = 202,
                ClassDescription = "Class 202 Description",
                ClassPoints = 2000,
                TeacherID = teacher1.TeacherID,
                Teacher = teacher1,
                WeeklyClassPoints = new List<WeeklyClassPoints>()
            };

            class2.WeeklyClassPoints = new List<WeeklyClassPoints> {
                new WeeklyClassPoints {
                    ClassID = "013e1876-281a-4db6-b0c8-3263ffbd0fd7",
                    Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                    PointsGained = 1000,
                    Class = class2
                },
                new WeeklyClassPoints {
                    ClassID = "013e1876-281a-4db6-b0c8-3263ffbd0fd7",
                    Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                    PointsGained = 2000,
                    Class = class2
                },
                new WeeklyClassPoints {
                    ClassID = "013e1876-281a-4db6-b0c8-3263ffbd0fd7",
                    Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                    PointsGained = 3000,
                    Class = class2
                }
            };

            var admin1 = new Admin {
                AdminID = "23145859-30c4-4b6c-b204-69dd897a6315",
                User = new User {
                    Id = "23145859-30c4-4b6c-b204-69dd897a6315",
                    Name = "Admin 1",
                    Email = "a@a.com",
                    Password = "adminPassword",
                    ContactNumber = "12345678",
                    UserRole = "admin",
                    Avatar = "avatar.jpg"
                }
            };

            var task1 = new Models.Task {
                TaskID = "1",
                TaskTitle = "Recycle 1 plastic bottle",
                TaskDescription = "Bring 1 plastic bottle to school and dispose it in the recycling bin.",
                TaskPoints = 100
            };

            var task2 = new Models.Task {
                TaskID = "2",
                TaskTitle = "Bring a set of newspapers to recycle",
                TaskDescription = "Bring a set of newspapers to school and dispose it in the recycling bin.",
                TaskPoints = 200
            };

            var task3 = new Models.Task {
                TaskID = "3",
                TaskTitle = "Bring reusable food containers",
                TaskDescription = "Bring reusable food containers to school and use them during recess.",
                TaskPoints = 300
            };

            AfterUserCreate(context, "teacher", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", teacher1.TeacherID },
                    { "Name", teacher1.TeacherName },
                    { "Email", "teacher1@example.com" },
                    { "Password", "teacherPassword" },
                    { "ContactNumber", "12345678" },
                    { "UserRole", "teacher" },
                    { "Avatar", "teacher_avatar.jpg" }
                }
            });

            AfterUserCreate(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student1.StudentID },
                    { "Name", "Student 1" },
                    { "Email", "student1@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "12345678" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" }
                }
            });

            AfterUserCreate(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student2.StudentID },
                    { "Name", "Student 2" },
                    { "Email", "student2@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "12345678" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" }
                }
            });

            context.Classes.Add(class1);
            context.Classes.Add(class2);
            context.Admins.Add(admin1);
            context.Tasks.Add(task1);
            context.Tasks.Add(task2);
            context.Tasks.Add(task3);

            await context.SaveChangesAsync();

            string dbMode = Environment.GetEnvironmentVariable("DB_MODE") ?? "";
            if (dbMode == "cloud") {
                context.SaveChanges();
            } else if (dbMode == "local") {
                SaveToSqlite(context);
            } else {
                throw new ArgumentException("Invalid DB_MODE configuration.");
            }
        }
    }
}
