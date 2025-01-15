using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

            string id = baseUser != "teacher" ? ValidateField(userDetails, "Id", required: true, "ID is required.") : "c1f76fc4-c99b-4517-9eac-c5ae54bb8808"; 
            string name = ValidateField(userDetails, "Name", required: true, "Name is required.");
            string email = ValidateEmail(userDetails.GetValueOrDefault("Email")?.ToString() ?? throw new ArgumentException("Email is required."), context);
            string password = ValidatePassword(userDetails.GetValueOrDefault("Password")?.ToString() ?? throw new ArgumentException("Password is required."));
            string contactNumber = ValidateContactNumber(userDetails.GetValueOrDefault("ContactNumber")?.ToString() ?? "", context);
            string userRole = ValidateField(userDetails, "UserRole", required: true, "UserRole is required.");
            string avatar = userDetails.GetValueOrDefault("Avatar")?.ToString() ?? "";

            var baseUserObj = new User
            {
                Id = id,
                Name = name,
                Email = email,
                Password = Utilities.HashString(password),
                ContactNumber = contactNumber,
                UserRole = userRole,
                Avatar = avatar
            };

            Console.WriteLine("Base User created: " + baseUserObj.Id);

            context.Users.Add(baseUserObj);
            await context.SaveChangesAsync();

            Console.WriteLine("Base User saved to database.");

            if (baseUser == "student") {
                var specificStudentObj = new Student {
                    StudentID = baseUserObj.Id
                };

                Console.WriteLine("Student created: " + specificStudentObj.StudentID);

                context.Students.Add(specificStudentObj);
            } else if (baseUser == "admin") {
                var specificAdminObj = new Admin {
                    AdminID = baseUserObj.Id,
                    User = baseUserObj
                };

                Console.WriteLine("Admin created: " + specificAdminObj.AdminID);

                context.Admins.Add(specificAdminObj);
            } else if (baseUser == "teacher") {
                var specificTeacherObj = new Teacher {
                    TeacherID = "c1f76fc4-c99b-4517-9eac-c5ae54bb8808",
                    TeacherName = baseUserObj.Name,
                    User = baseUserObj
                };

                Console.WriteLine("Teacher created: " + specificTeacherObj.TeacherID);

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

                    Console.WriteLine("Parent created: " + specificParentObj.ParentID);

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
            // Clear existing data
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

            await CreateUserRecords(context, "teacher", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", "c1f76fc4-c99b-4517-9eac-c5ae54bb8808" },
                    { "Name", "Teacher 1" },
                    { "Email", "teacher1@example.com" },
                    { "Password", "teacherPassword" },
                    { "ContactNumber", "11111111" },
                    { "UserRole", "teacher" },
                    { "Avatar", "teacher_avatar.jpg" }
                }
            });

            // Then create students
            var student1Id = "73ecc6b8-805e-46ff-bbc3-bec52073e25d";
            var student2Id = "3f9056b0-06e1-487a-8901-586bafd1e492";
            
            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student1Id },
                    { "Name", "Student 1" },
                    { "Email", "student1@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "22222222" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student2Id },
                    { "Name", "Student 2" },
                    { "Email", "student2@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "33333333" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" }
                }
            });

            var teacher1 = context.Teachers.FirstOrDefault(t => t.TeacherID == "c1f76fc4-c99b-4517-9eac-c5ae54bb8808");
            
            var class1 = new Class {
                ClassID = "ca4daece-c27c-46a1-99d5-1c8fd650165e",
                ClassName = 101,
                ClassDescription = "Class 202 Description",
                ClassPoints = 1000,
                TeacherID = "c1f76fc4-c99b-4517-9eac-c5ae54bb8808",
                Teacher = teacher1,
                WeeklyClassPoints = new List<WeeklyClassPoints>()
            };

            var class2 = new Class {
                ClassID = "013e1876-281a-4db6-b0c8-3263ffbd0fd7",
                ClassName = 202,
                ClassDescription = "Class 202 Description",
                ClassPoints = 2000,
                TeacherID = "c1f76fc4-c99b-4517-9eac-c5ae54bb8808",
                Teacher = teacher1,
                WeeklyClassPoints = new List<WeeklyClassPoints>()
            };

            // Add weekly points after classes are created
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

            // Save all changes
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
