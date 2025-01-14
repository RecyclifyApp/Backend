using Microsoft.EntityFrameworkCore;
using Backend.Models;

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