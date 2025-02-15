using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;

namespace Backend.Services {
    public class DatabaseManager {
        public static void SaveToSqlite(MyDbContext context) {
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

        public static string ValidateUsername(string username, MyDbContext context) {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.");
            if (context.Users.Any(u => u.Name == username))
                throw new ArgumentException("Username must be unique.");
            return username;
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

            string id;
            if (userDetails.ContainsKey("Id")) {
                id = keyValuePairs[0]["Id"].ToString() ?? "";
            } else {
                id = Utilities.GenerateUniqueID();
            }
            string name = ValidateUsername(userDetails.GetValueOrDefault("Name")?.ToString() ?? throw new ArgumentException("Username is required."), context);
            string fname = ValidateField(userDetails, "FName", required: true, "FName is required.");
            string lname = ValidateField(userDetails, "LName", required: true, "LName is required.");
            string email = ValidateEmail(userDetails.GetValueOrDefault("Email")?.ToString() ?? throw new ArgumentException("Email is required."), context);
            string password = ValidatePassword(userDetails.GetValueOrDefault("Password")?.ToString() ?? throw new ArgumentException("Password is required."));
            string contactNumber = ValidateContactNumber(userDetails.GetValueOrDefault("ContactNumber")?.ToString() ?? "", context);
            string userRole = ValidateField(userDetails, "UserRole", required: true, "UserRole is required.");
            string avatar = userDetails.GetValueOrDefault("Avatar")?.ToString() ?? "";
            string linkedStudent = userDetails.GetValueOrDefault("StudentID")?.ToString() ?? "";

            var baseUserObj = new User {
                Id = id,
                Name = name,
                FName = fname,
                LName = lname,
                Email = email,
                Password = Utilities.HashString(password),
                ContactNumber = contactNumber,
                UserRole = userRole,
                Avatar = avatar,
                EmailVerified = false,
                PhoneVerified = false
            };

            if (baseUser == "student") {
                var generateCurrentPoints = Utilities.GenerateRandomInt(0, 500);
                var specificStudentObj = new Student {
                    UserID = baseUserObj.Id,
                    StudentID = baseUserObj.Id,
                    Streak = 7,
                    League = new[] { "Bronze", "Silver", "Gold" }[new Random().Next(3)],
                    CurrentPoints = generateCurrentPoints,
                    TotalPoints = generateCurrentPoints + Utilities.GenerateRandomInt(0, 1000),
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
                var studentFound = context.Students.Find(linkedStudent);
                if (studentFound == null) {
                    throw new ArgumentException("Invalid student ID.");
                } else {
                    var specificParentObj = new Parent {
                        ParentID = baseUserObj.Id,
                        StudentID = keyValuePairs[0]["StudentID"].ToString() ?? "",
                        User = baseUserObj,
                        Student = studentFound
                    };

                    studentFound.ParentID = baseUserObj.Id;

                    context.Parents.Add(specificParentObj);
                    context.Students.Update(studentFound);
                }
            } else {
                throw new ArgumentException("Invalid user role.");
            }

            context.Users.Add(baseUserObj);
            await context.SaveChangesAsync();

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