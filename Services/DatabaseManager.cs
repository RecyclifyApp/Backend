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

            // string id = baseUser == "teacher" ? "c1f76fc4-c99b-4517-9eac-c5ae54bb8808" : Utilities.GenerateUniqueID();
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
                EmailVerified = false
            };

            if (baseUser == "student") {
                var generateCurrentPoints = Utilities.GenerateRandomInt(0, 500);
                var specificStudentObj = new Student {
                    UserID = baseUserObj.Id,
                    StudentID = baseUserObj.Id,
                    Streak = Utilities.GenerateRandomInt(0, 10),
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

        public static async Task CleanAndPopulateDatabase(MyDbContext context) {
            context.Admins.RemoveRange(context.Admins);
            context.Classes.RemoveRange(context.Classes);
            context.ClassPoints.RemoveRange(context.ClassPoints);
            context.ClassStudents.RemoveRange(context.ClassStudents);
            context.ContactForms.RemoveRange(context.ContactForms);
            context.DailyStudentPoints.RemoveRange(context.DailyStudentPoints);
            context.Inboxes.RemoveRange(context.Inboxes);
            context.Parents.RemoveRange(context.Parents);
            context.Quests.RemoveRange(context.Quests);
            context.QuestProgresses.RemoveRange(context.QuestProgresses);
            context.Redemptions.RemoveRange(context.Redemptions);
            context.RewardItems.RemoveRange(context.RewardItems);
            context.Students.RemoveRange(context.Students);
            context.StudentPoints.RemoveRange(context.StudentPoints);
            context.Tasks.RemoveRange(context.Tasks);
            context.TaskProgresses.RemoveRange(context.TaskProgresses);
            context.Teachers.RemoveRange(context.Teachers);
            context.Users.RemoveRange(context.Users);
            context.WeeklyClassPoints.RemoveRange(context.WeeklyClassPoints);

            await context.SaveChangesAsync();

            await CreateUserRecords(context, "admin", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Name", "John Appleseed" },
                    { "FName", "John" },
                    { "LName", "Appleseed" },
                    { "Email", "johnappleseed@example.com" },
                    { "Password", "adminPassword" },
                    { "ContactNumber", "00000000" },
                    { "UserRole", "admin" },
                    { "Avatar", "admin_avatar.jpg" },
                    { "EmailVerified", false }
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
                    { "Avatar", "teacher_avatar.jpg" },
                    { "EmailVerified", false }
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
                WeeklyClassPoints = new List<WeeklyClassPoints>(),
                JoinCode = Utilities.GenerateRandomInt(100000, 999999)
            };

            var class2 = new Class {
                ClassID = Utilities.GenerateUniqueID(),
                ClassName = 202,
                ClassDescription = "Class 202 Description",
                ClassPoints = 2000,
                TeacherID = teacher1?.TeacherID ?? throw new ArgumentNullException(nameof(teacher1), "Teacher not found."),
                Teacher = teacher1 ?? throw new ArgumentNullException(nameof(teacher1), "Teacher not found."),
                WeeklyClassPoints = new List<WeeklyClassPoints>(),
                JoinCode = Utilities.GenerateRandomInt(100000, 999999)
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

            var student1Id = Utilities.GenerateUniqueID();
            var student2Id = Utilities.GenerateUniqueID();
            var student3Id = Utilities.GenerateUniqueID();
            var student4Id = Utilities.GenerateUniqueID();
            var student5Id = Utilities.GenerateUniqueID();
            var student6Id = Utilities.GenerateUniqueID();
            var student7Id = Utilities.GenerateUniqueID();
            var student8Id = Utilities.GenerateUniqueID();
            var student9Id = Utilities.GenerateUniqueID();
            var student10Id = Utilities.GenerateUniqueID();
            
            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student1Id },
                    { "Name", "Lana Ng" },
                    { "FName", "Lana" },
                    { "LName", "Ng" },
                    { "Email", "lanang@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "22222222" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student2Id },
                    { "Name", "Kate Gibson" },
                    { "FName", "Kate" },
                    { "LName", "Gibson" },
                    { "Email", "kategibson@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "33333333" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student3Id },
                    { "Name", "Peter Parker" },
                    { "FName", "Peter" },
                    { "LName", "Parker" },
                    { "Email", "peterparker@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "44444444" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student4Id },
                    { "Name", "Ethan Carter" },
                    { "FName", "Ethan" },
                    { "LName", "Carter" },
                    { "Email", "ethancarter@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "55555555" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student5Id },
                    { "Name", "Olivia Bennett" },
                    { "FName", "Olivia" },
                    { "LName", "Bennett" },
                    { "Email", "oliviabennett@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "66666666" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student6Id },
                    { "Name", "Noah Mitchell" },
                    { "FName", "Noah" },
                    { "LName", "Mitchell" },
                    { "Email", "noahmitchell@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "77777777" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student7Id },
                    { "Name", "Emma Robinson" },
                    { "FName", "Emma" },
                    { "LName", "Robinson" },
                    { "Email", "emmarobinson@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "88888888" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student8Id },
                    { "Name", "Liam Turner" },
                    { "FName", "Liam" },
                    { "LName", "Turner" },
                    { "Email", "liamturner@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "99999999" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student9Id },
                    { "Name", "Ava Parker" },
                    { "FName", "Ava" },
                    { "LName", "Parker" },
                    { "Email", "avaparker@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "10101010" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await CreateUserRecords(context, "student", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Id", student10Id },
                    { "Name", "Sophia Ramirez" },
                    { "FName", "Sophia" },
                    { "LName", "Ramirez" },
                    { "Email", "sophiaramirez@example.com" },
                    { "Password", "studentPassword" },
                    { "ContactNumber", "12121212" },
                    { "UserRole", "student" },
                    { "Avatar", "student_avatar.jpg" },
                    { "EmailVerified", false }
                }
            });

            await context.SaveChangesAsync();

            for (int i = 1; i <= 10; i++) {
                var studentId = context.Students.ToList()[i - 1].StudentID;
                var class1Students = new ClassStudents {
                    ClassID = class1.ClassID,
                    StudentID = studentId
                };

                var class2Students = new ClassStudents {
                    ClassID = class2.ClassID,
                    StudentID = studentId
                };

                context.ClassStudents.Add(class1Students);
                context.ClassStudents.Add(class2Students);
            }

            var quest1ID = Utilities.GenerateUniqueID();
            var quest2ID = Utilities.GenerateUniqueID();
            var quest3ID = Utilities.GenerateUniqueID();
            var quest4ID = Utilities.GenerateUniqueID();
            var quest5ID = Utilities.GenerateUniqueID();
            var quest6ID = Utilities.GenerateUniqueID();
            var quest7ID = Utilities.GenerateUniqueID();
            var quest8ID = Utilities.GenerateUniqueID();
            var quest9ID = Utilities.GenerateUniqueID();
            var quest10ID = Utilities.GenerateUniqueID();
            var quest11ID = Utilities.GenerateUniqueID();
            var quest12ID = Utilities.GenerateUniqueID();
            var quest13ID = Utilities.GenerateUniqueID();
            var quest14ID = Utilities.GenerateUniqueID();
            var quest15ID = Utilities.GenerateUniqueID();
            var quest16ID = Utilities.GenerateUniqueID();
            var quest17ID = Utilities.GenerateUniqueID();
            var quest18ID = Utilities.GenerateUniqueID();
            var quest19ID = Utilities.GenerateUniqueID();
            var quest20ID = Utilities.GenerateUniqueID();

            for (int i = 0; i < 20; i++) {
                var quest = new Quest {
                    QuestID = i switch {
                        0 => quest1ID,
                        1 => quest2ID,
                        2 => quest3ID,
                        3 => quest4ID,
                        4 => quest5ID,
                        5 => quest6ID,
                        6 => quest7ID,
                        7 => quest8ID,
                        8 => quest9ID,
                        9 => quest10ID,
                        10 => quest11ID,
                        11 => quest12ID,
                        12 => quest13ID,
                        13 => quest14ID,
                        14 => quest15ID,
                        15 => quest16ID,
                        16 => quest17ID,
                        17 => quest18ID,
                        18 => quest19ID,
                        19 => quest20ID,
                        _ => ""
                    },
                    QuestTitle = $"Quest {i + 1}",
                    QuestDescription = i switch {
                        0 => "Bring 30 plastic bottles for recycling.",
                        1 => "Sort and recycle 20 used cans.",
                        2 => "Recycle 50 sheets of paper.",
                        3 => "Collect 20 used batteries for recycling.",
                        4 => "Bring 3 plastic containers to school for recycling.",
                        5 => "Collect 30 cardboard items and recycle them.",
                        6 => "Collect 10 empty cans and bring them to the recycling bin.",
                        
                        7 => "5 students turn off all lights in your home when not in use for a day.",
                        8 => "5 students track and reduce your electricity usage for a day.",
                        9 => "10 students use natural sunlight instead of electrical lights for a day.",
                        10 => "5 students use energy-saving appliances for a day.",
                        11 => "5 students engage in outdoor activities instead of using electronic devices for a day.",
                        12 => "10 students use fans instead of air-conditioning for a day.",
                        
                        13 => "10 students plant 1 small plant at home.",
                        14 => "Pick up 30 pieces of litter from surroundings.",
                        15 => "10 students use re-usable cutlery for a day.",
                        16 => "10 students bring their own re-usable food containers for a day.",
                        17 => "10 students help water the plants in the school garden.",
                        18 => "Create 10 posters on how to reduce waste at school.",
                        19 => "10 students walk or cycle to school for a day.",
                        _ => ""
                    },
                    QuestPoints = i switch {
                        <= 6 => 100,
                        <= 12 => 200,
                        _ => 300
                    },
                    QuestType = i switch {
                        <= 6 => "Recycling",
                        <= 12 => "Energy",
                        _ => "Environment"
                    }
                };

                context.Quests.Add(quest);
            }

            for (int i = 0; i < 20; i++) {
                var task = new Models.Task {
                    TaskID = Utilities.GenerateUniqueID(),
                    TaskTitle = $"Task {i + 1}",
                    TaskDescription = i switch {
                        0 => "Bring 5 plastic bottles for recycling.",
                        1 => "Sort and recycle 10 used cans.",
                        2 => "Recycle 20 sheets of paper.",
                        3 => "Collect 5 used batteries for recycling.",
                        4 => "Bring 3 plastic containers to school for recycling.",
                        5 => "Collect 10 cardboard items and recycle them.",
                        6 => "Collect 5 empty cans and bring them to the recycling bin.",
                        
                        7 => "Turn off all lights in your home when not in use for a day.",
                        8 => "Track and reduce your electricity usage for a day.",
                        9 => "Use natural sunlight instead of electrical lights for a day.",
                        10 => "Use energy-saving appliances for a day.",
                        11 => "Engage in outdoor activities instead of using electronic devices for a day.",
                        12 => "Use fans instead of air-conditioning for a day.",
                        
                        13 => "Plant 1 small plant at home.",
                        14 => "Pick up 10 pieces of litter from your surroundings.",
                        15 => "Use re-usable cutlery for a day.",
                        16 => "Bring your own re-usable food containers for a day.",
                        17 => "Help water the plants in the school garden.",
                        18 => "Create 1 poster on how to reduce waste at school.",
                        19 => "Walk or cycle to school for a day.",
                        _ => ""
                    },
                    TaskPoints = i switch {
                        <= 6 => 30,
                        <= 12 => 50,
                        _ => 75
                    },
                    AssociatedQuestID = i switch {
                        0 => quest1ID,
                        1 => quest2ID,
                        2 => quest3ID,
                        3 => quest4ID,
                        4 => quest5ID,
                        5 => quest6ID,
                        6 => quest7ID,
                        7 => quest8ID,
                        8 => quest9ID,
                        9 => quest10ID,
                        10 => quest11ID,
                        11 => quest12ID,
                        12 => quest13ID,
                        13 => quest14ID,
                        14 => quest15ID,
                        15 => quest16ID,
                        16 => quest17ID,
                        17 => quest18ID,
                        18 => quest19ID,
                        19 => quest20ID,
                        _ => ""
                    }
                };

                context.Tasks.Add(task);
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

                var student3Points = new StudentPoints {
                    StudentID = student3Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var student4Points = new StudentPoints {
                    StudentID = student4Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var student5Points = new StudentPoints {
                    StudentID = student5Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var student6Points = new StudentPoints {
                    StudentID = student6Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var student7Points = new StudentPoints {
                    StudentID = student7Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var student8Points = new StudentPoints {
                    StudentID = student8Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var student9Points = new StudentPoints {
                    StudentID = student9Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var student10Points = new StudentPoints {
                    StudentID = student10Id,
                    TaskID = context.Tasks.ToList()[i].TaskID,
                    DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                context.StudentPoints.Add(student1Points);
                context.StudentPoints.Add(student2Points);
                context.StudentPoints.Add(student3Points);
                context.StudentPoints.Add(student4Points);
                context.StudentPoints.Add(student5Points);
                context.StudentPoints.Add(student6Points);
                context.StudentPoints.Add(student7Points);
                context.StudentPoints.Add(student8Points);
                context.StudentPoints.Add(student9Points);
                context.StudentPoints.Add(student10Points);
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

            await CreateUserRecords(context, "parent", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Name", "Nicholas Chew" },
                    { "FName", "Nicholas" },
                    { "LName", "Chew" },
                    { "Email", "lincolnlim267@gmail.com" },
                    { "Password", "parentPassword" },
                    { "ContactNumber", "12312312" },
                    { "UserRole", "parent" },
                    { "Avatar", "parent_avatar.jpg" },
                    { "EmailVerified", false },
                    { "StudentID", student1Id }
                }
            });

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