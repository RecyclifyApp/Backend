using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;

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

        public static string ValidateField(Dictionary<string, object> userDetails, string key, bool required, string errorMessage) {
            string value = userDetails.GetValueOrDefault(key)?.ToString() ?? "";
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(errorMessage);
            return value ?? "";
        }

        public static string ValidateEmail(string email, MyDbContext context) {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email)) {
                throw new ArgumentException("Invalid email format."); 
            }
            if (context.Users.Any(u => u.Email == email)) {
                throw new ArgumentException("Email must be unique.");
            }
            return email;
        }

        public static string ValidatePassword(string password) {
            if (password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long.");
            return password;
        }

        public static string ValidateContactNumber(string contactNumber, MyDbContext context) {
            if (!string.IsNullOrWhiteSpace(contactNumber)) {
                var phoneRegex = new Regex(@"^\+?\d{8}$");
                if (!phoneRegex.IsMatch(contactNumber))
                    throw new ArgumentException("Invalid contact number format.");

                if (context.Users.Any(u => u.ContactNumber == contactNumber))
                    throw new ArgumentException("Contact number must be unique.");
            }
            return contactNumber;
        }

        public static async Task CreateUserRecords(MyDbContext context, string baseUser, List<Dictionary<string, object>> keyValuePairs) {
            var userDetails = keyValuePairs[0];

            string id = keyValuePairs[0]["Id"].ToString() ?? Utilities.GenerateUniqueID();
            string name = ValidateField(userDetails, "Name", required: true, "Name is required.");
            string fname = ValidateField(userDetails, "FName", required: true, "First name is required.");
            string lname = ValidateField(userDetails, "LName", required: true, "Last name is required.");
            string email = ValidateEmail(userDetails.GetValueOrDefault("Email")?.ToString() ?? throw new ArgumentException("Email is required."), context);
            string password = ValidatePassword(userDetails.GetValueOrDefault("Password")?.ToString() ?? throw new ArgumentException("Password is required."));
            string contactNumber = ValidateContactNumber(userDetails.GetValueOrDefault("ContactNumber")?.ToString() ?? "", context);
            string userRole = ValidateField(userDetails, "UserRole", required: true, "UserRole is required.");
            string avatar = userDetails.GetValueOrDefault("Avatar")?.ToString() ?? "";

            var baseUserObj = new User {
                Id = id,
                Name = name,
                FName = fname,
                LName = lname,
                Email = email,
                Password = Utilities.HashString(password),
                ContactNumber = contactNumber,
                UserRole = userRole,
                Avatar = avatar
            };

            context.Users.Add(baseUserObj);
            await context.SaveChangesAsync();

            if (baseUser == "student") {
                var specificStudentObj = new Student {
                    StudentID = baseUserObj.Id,
                    ClassID = keyValuePairs[0]["ClassID"].ToString() ?? null,
                };

                context.Students.Add(specificStudentObj);
            } else if (baseUser == "admin") {
                var specificAdminObj = new Admin {
                    AdminID = baseUserObj.Id,
                    User = baseUserObj
                };

                context.Admins.Add(specificAdminObj);
            } else if (baseUser == "teacher") {
                var specificTeacherObj = new Teacher {
                    TeacherID = baseUserObj.Id,
                    TeacherName = baseUserObj.Name,
                    User = baseUserObj
                };

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

                    context.Parents.Add(specificParentObj);
                }
            } else {
                throw new ArgumentException("Invalid user role.");
            }

            string dbMode = Environment.GetEnvironmentVariable("DB_MODE") ?? "";
            if (dbMode == "cloud") {
                await context.SaveChangesAsync();
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
            
            await context.SaveChangesAsync();

            await CreateUserRecords(context, "admin", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", Utilities.GenerateUniqueID() },
                    { "Name", "John Appleseed" },
                    { "FName", "John" },
                    { "LName", "Appleseed" },
                    { "Email", "johnappleseed@example.com" },
                    { "Password", "adminPassword" },
                    { "ContactNumber", "00000000" },
                    { "UserRole", "admin" },
                    { "Avatar", "admin_avatar.jpg" }
                }
            });

            await CreateUserRecords(context, "teacher", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", "c1f76fc4-c99b-4517-9eac-c5ae54bb8808" },
                    { "Name", "Lincoln Lim" },
                    { "FName", "Lincoln" },
                    { "LName", "Lim" },
                    { "Email", "lincolnlim@example.com" },
                    { "Password", "teacherPassword" },
                    { "ContactNumber", "11111111" },
                    { "UserRole", "teacher" },
                    { "Avatar", "teacher_avatar.jpg" }
                }
            });

            var teacher1 = context.Teachers.FirstOrDefault(t => t.TeacherID == "c1f76fc4-c99b-4517-9eac-c5ae54bb8808");
            
            var class1 = new Class {
                ClassID = Utilities.GenerateUniqueID(),
                ClassName = 101,
                ClassDescription = "Class 101 Description",
                ClassPoints = 1000,
                TeacherID = teacher1?.TeacherID ?? throw new ArgumentNullException(nameof(teacher1), "Teacher not found."),
                Teacher = teacher1 ?? throw new ArgumentNullException(nameof(teacher1), "Teacher not found."),
                WeeklyClassPoints = new List<WeeklyClassPoints>()
            };

            var class2 = new Class {
                ClassID = Utilities.GenerateUniqueID(),
                ClassName = 202,
                ClassDescription = "Class 202 Description",
                ClassPoints = 2000,
                TeacherID = teacher1?.TeacherID ?? throw new ArgumentNullException(nameof(teacher1), "Teacher not found."),
                Teacher = teacher1 ?? throw new ArgumentNullException(nameof(teacher1), "Teacher not found."),
                WeeklyClassPoints = new List<WeeklyClassPoints>()
            };

            class1.WeeklyClassPoints = new List<WeeklyClassPoints> {
                new WeeklyClassPoints {
                    ClassID = class1.ClassID,
                    Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                    PointsGained = 1000,
                    Class = class1
                },
            };

            class2.WeeklyClassPoints = new List<WeeklyClassPoints> {
                new WeeklyClassPoints {
                    ClassID = class2.ClassID,
                    Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                    PointsGained = 1000,
                    Class = class2
                },
            };

            context.Classes.Add(class1);
            context.Classes.Add(class2);
            await context.SaveChangesAsync();

            var student1Id = "73ecc6b8-805e-46ff-bbc3-bec52073e25d";
            var student2Id = "3f9056b0-06e1-487a-8901-586bafd1e492";
            
            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student1Id },
                    { "ClassID", class1.ClassID },
                    { "Name", "Lana Ng" },
                    { "FName", "Lana" },
                    { "LName", "Ng" },
                    { "Email", "lanang@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "22222222" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student2Id },
                    { "ClassID", class2.ClassID },
                    { "Name", "Kate Gibson" },
                    { "FName", "Kate" },
                    { "LName", "Gibson" },
                    { "Email", "kategibson@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "33333333" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" }
                }
            });

            for (int i = 0; i < 20; i++) {
                var task = new Models.Task {
                    TaskID = Utilities.GenerateUniqueID(),
                    TaskTitle = $"Task {i + 1}",
                    TaskDescription = $"Task {i + 1} Description",
                    TaskPoints = 100,
                };

                context.Tasks.Add(task);
            }

            for (int i = 0; i < 20; i++) {
                var quest = new Quest {
                    QuestID = Utilities.GenerateUniqueID(),
                    QuestTitle = $"Quest {i + 1}",
                    QuestDescription = $"Quest {i + 1} Description",
                    QuestPoints = 100,
                };

                context.Quests.Add(quest);
            }

            await context.SaveChangesAsync();

            for (int i = 0; i < 10; i++) {
                var student1Points = new StudentPoints {
                    StudentID = student1Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var student2Points = new StudentPoints {
                    StudentID = student2Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                context.StudentPoints.Add(student1Points);
                context.StudentPoints.Add(student2Points);
            }

            for (int i = 0; i < 10; i++) {
                var class1Points = new ClassPoints {
                    ClassID = class1.ClassID,
                    QuestID = context.Quests.ToList()[i].QuestID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var class2Points = new ClassPoints {
                    ClassID = class2.ClassID,
                    QuestID = context.Quests.ToList()[i].QuestID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                context.ClassPoints.Add(class1Points);
                context.ClassPoints.Add(class2Points);
            }

            for (int i = 0; i < 10; i++) {
                var rewardItem = new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = $"Reward {i + 1}",
                    RewardDescription = $"Reward {i + 1} Description",
                    RequiredPoints = Utilities.GenerateRandomInt(100, 1000),
                    RewardQuantity = Utilities.GenerateRandomInt(1, 10),
                    IsAvailable = true
                };

                context.RewardItems.Add(rewardItem);
            }

            string dbMode = Environment.GetEnvironmentVariable("DB_MODE") ?? "";
            if (dbMode == "cloud") {
                await context.SaveChangesAsync();
            } else if (dbMode == "local") {
                SaveToSqlite(context);
            } else {
                throw new ArgumentException("Invalid DB_MODE configuration.");
            }
        }
    }
}
