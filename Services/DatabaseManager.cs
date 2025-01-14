using Microsoft.EntityFrameworkCore;
using Backend.Models;
using System.Text.RegularExpressions;

namespace Backend.Services {
    public static class DatabaseManager {
        public static void SaveToSqlite(MyDbContext context) { // Use this method to save data to local SQLite database, instead of traditionally just using context.SaveChanges()
            context.SaveChanges();
            context.Database.ExecuteSqlInterpolated($"PRAGMA wal_checkpoint(FULL)");
            context.Dispose();
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

        private static string ValidateField(Dictionary<string, object> userDetails, string key, bool required, string errorMessage) {
            string value = userDetails.GetValueOrDefault(key)?.ToString() ?? "";
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(errorMessage);
            return value ?? "";
        }

        private static string ValidateEmail(string email, MyDbContext context) {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email)) {
                throw new ArgumentException("Invalid email format.");
            }
            if (context.Users.Any(u => u.Email == email)) {
                throw new ArgumentException("Email must be unique.");
            }
            return email;
        }

        private static string ValidatePassword(string password) {
            if (password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long.");
            return password;
        }

        private static string ValidateContactNumber(string contactNumber, MyDbContext context) {
            if (!string.IsNullOrWhiteSpace(contactNumber)) {
                var phoneRegex = new Regex(@"^\+?\d{8}$");
                if (!phoneRegex.IsMatch(contactNumber))
                    throw new ArgumentException("Invalid contact number format.");

                if (context.Users.Any(u => u.ContactNumber == contactNumber))
                    throw new ArgumentException("Contact number must be unique.");
            }
            return contactNumber;
        }

        public static void CreateUserRecords(MyDbContext context, string baseUser, List<Dictionary<string, object>> keyValuePairs) {
            var userDetails = keyValuePairs[0];

            string id = Guid.NewGuid().ToString(); 
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
                var specificTeacehrObj = new Teacher {
                    TeacherID = baseUserObj.Id,
                    TeacherName = baseUserObj.Name
                };

                context.Users.Add(baseUserObj);
                context.Teachers.Add(specificTeacehrObj);
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
    }
}