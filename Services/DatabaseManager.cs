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

            for (int i = 1; i <= 5; i++) {
                var studentId = context.Students.ToList()[i - 1].StudentID;
                var class1Students = new ClassStudents {
                    ClassID = class1.ClassID,
                    StudentID = studentId
                };

                context.ClassStudents.Add(class1Students);
            }

            for (int i = 6; i <= 10; i++) {
                var studentId = context.Students.ToList()[i - 1].StudentID;
                var class2Students = new ClassStudents {
                    ClassID = class2.ClassID,
                    StudentID = studentId
                };

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

            var quest1 = new Quest {
                QuestID = quest1ID,
                QuestTitle = "Quest 1",
                QuestDescription = "Bring 30 plastic bottles for recycling.",
                QuestPoints = 100,
                QuestType = "Recycling",
                TotalAmountToComplete = 30
            };

            var quest2 = new Quest {
                QuestID = quest2ID,
                QuestTitle = "Quest 2",
                QuestDescription = "Sort and recycle 20 used cans.",
                QuestPoints = 100,
                QuestType = "Recycling",
                TotalAmountToComplete = 20
            };

            var quest3 = new Quest {
                QuestID = quest3ID,
                QuestTitle = "Quest 3",
                QuestDescription = "Recycle 50 sheets of paper.",
                QuestPoints = 100,
                QuestType = "Recycling",
                TotalAmountToComplete = 50
            };

            var quest4 = new Quest {
                QuestID = quest4ID,
                QuestTitle = "Quest 4",
                QuestDescription = "Collect 20 used batteries for recycling.",
                QuestPoints = 100,
                QuestType = "Recycling",
                TotalAmountToComplete = 20
            };

            var quest5 = new Quest {
                QuestID = quest5ID,
                QuestTitle = "Quest 5",
                QuestDescription = "Bring 30 plastic containers to school for recycling.",
                QuestPoints = 100,
                QuestType = "Recycling",
                TotalAmountToComplete = 30
            };

            var quest6 = new Quest {
                QuestID = quest6ID,
                QuestTitle = "Quest 6",
                QuestDescription = "Collect 30 cardboard items and recycle them.",
                QuestPoints = 100,
                QuestType = "Recycling",
                TotalAmountToComplete = 30
            };

            var quest7 = new Quest {
                QuestID = quest7ID,
                QuestTitle = "Quest 7",
                QuestDescription = "Collect 10 empty cans and bring them to the recycling bin.",
                QuestPoints = 100,
                QuestType = "Recycling",
                TotalAmountToComplete = 10
            };

            var quest8 = new Quest {
                QuestID = quest8ID,
                QuestTitle = "Quest 8",
                QuestDescription = "5 students turn off all lights in your home when not in use for a day.",
                QuestPoints = 200,
                QuestType = "Energy",
                TotalAmountToComplete = 5
            };

            var quest9 = new Quest {
                QuestID = quest9ID,
                QuestTitle = "Quest 9",
                QuestDescription = "5 students track and reduce your electricity usage for a day.",
                QuestPoints = 200,
                QuestType = "Energy",
                TotalAmountToComplete = 5
            };

            var quest10 = new Quest {
                QuestID = quest10ID,
                QuestTitle = "Quest 10",
                QuestDescription = "10 students use natural sunlight instead of electrical lights for a day.",
                QuestPoints = 200,
                QuestType = "Energy",
                TotalAmountToComplete = 10
            };

            var quest11 = new Quest {
                QuestID = quest11ID,
                QuestTitle = "Quest 11",
                QuestDescription = "5 students use energy-saving appliances for a day.",
                QuestPoints = 200,
                QuestType = "Energy",
                TotalAmountToComplete = 5
            };

            var quest12 = new Quest {
                QuestID = quest12ID,
                QuestTitle = "Quest 12",
                QuestDescription = "5 students engage in outdoor activities instead of using electronic devices for a day.",
                QuestPoints = 200,
                QuestType = "Energy",
                TotalAmountToComplete = 5
            };

            var quest13 = new Quest {
                QuestID = quest13ID,
                QuestTitle = "Quest 13",
                QuestDescription = "10 students use fans instead of air-conditioning for a day.",
                QuestPoints = 200,
                QuestType = "Energy",
                TotalAmountToComplete = 10
            };

            var quest14 = new Quest {
                QuestID = quest14ID,
                QuestTitle = "Quest 14",
                QuestDescription = "10 students plant 1 small plant at home.",
                QuestPoints = 300,
                QuestType = "Environment",
                TotalAmountToComplete = 10
            };

            var quest15 = new Quest {
                QuestID = quest15ID,
                QuestTitle = "Quest 15",
                QuestDescription = "Pick up 30 pieces of litter from surroundings.",
                QuestPoints = 300,
                QuestType = "Environment",
                TotalAmountToComplete = 30
            };

            var quest16 = new Quest {
                QuestID = quest16ID,
                QuestTitle = "Quest 16",
                QuestDescription = "10 students use re-usable cutlery for a day.",
                QuestPoints = 300,
                QuestType = "Environment",
                TotalAmountToComplete = 10
            };

            var quest17 = new Quest {
                QuestID = quest17ID,
                QuestTitle = "Quest 17",
                QuestDescription = "10 students bring their own re-usable food containers for a day.",
                QuestPoints = 300,
                QuestType = "Environment",
                TotalAmountToComplete = 10
            };

            var quest18 = new Quest {
                QuestID = quest18ID,
                QuestTitle = "Quest 18",
                QuestDescription = "10 students help water the plants in the school garden.",
                QuestPoints = 300,
                QuestType = "Environment",
                TotalAmountToComplete = 10
            };

            var quest19 = new Quest {
                QuestID = quest19ID,
                QuestTitle = "Quest 19",
                QuestDescription = "Create 10 posters on how to reduce waste at school.",
                QuestPoints = 300,
                QuestType = "Environment",
                TotalAmountToComplete = 10
            };

            var quest20 = new Quest {
                QuestID = quest20ID,
                QuestTitle = "Quest 20",
                QuestDescription = "10 students walk or cycle to school for a day.",
                QuestPoints = 300,
                QuestType = "Environment",
                TotalAmountToComplete = 10
            };

            context.Quests.AddRange(quest1, quest2, quest3, quest4, quest5, quest6, quest7, quest8, quest9, quest10, quest11, quest12, quest13, quest14, quest15, quest16, quest17, quest18, quest19, quest20);

            var task1 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 1",
                TaskDescription = "Bring 5 plastic bottles for recycling.",
                TaskPoints = 30,
                AssociatedQuestID = quest1ID,
                QuestContributionAmountOnComplete = 5
            };

            var task2 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 2",
                TaskDescription = "Sort and recycle 10 used cans.",
                TaskPoints = 30,
                AssociatedQuestID = quest2ID,
                QuestContributionAmountOnComplete = 10
            };

            var task3 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 3",
                TaskDescription = "Recycle 20 sheets of paper.",
                TaskPoints = 30,
                AssociatedQuestID = quest3ID,
                QuestContributionAmountOnComplete = 20
            };

            var task4 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 4",
                TaskDescription = "Collect 5 used batteries for recycling.",
                TaskPoints = 30,
                AssociatedQuestID = quest4ID,
                QuestContributionAmountOnComplete = 5
            };

            var task5 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 5",
                TaskDescription = "Bring 3 plastic containers to school for recycling.",
                TaskPoints = 30,
                AssociatedQuestID = quest5ID,
                QuestContributionAmountOnComplete = 3
            };

            var task6 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 6",
                TaskDescription = "Collect 10 cardboard items and recycle them.",
                TaskPoints = 30,
                AssociatedQuestID = quest6ID,
                QuestContributionAmountOnComplete = 10
            };

            var task7 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 7",
                TaskDescription = "Collect 5 empty cans and bring them to the recycling bin.",
                TaskPoints = 30,
                AssociatedQuestID = quest7ID,
                QuestContributionAmountOnComplete = 5
            };

            var task8 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 8",
                TaskDescription = "Turn off all lights in your home when not in use for a day.",
                TaskPoints = 50,
                AssociatedQuestID = quest8ID,
                QuestContributionAmountOnComplete = 1
            };

            var task9 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 9",
                TaskDescription = "Track and reduce your electricity usage for a day.",
                TaskPoints = 50,
                AssociatedQuestID = quest9ID,
                QuestContributionAmountOnComplete = 1
            };

            var task10 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 10",
                TaskDescription = "Use natural sunlight instead of electrical lights for a day.",
                TaskPoints = 50,
                AssociatedQuestID = quest10ID,
                QuestContributionAmountOnComplete = 1
            };

            var task11 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 11",
                TaskDescription = "Use energy-saving appliances for a day.",
                TaskPoints = 50,
                AssociatedQuestID = quest11ID,
                QuestContributionAmountOnComplete = 1
            };

            var task12 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 12",
                TaskDescription = "Engage in outdoor activities instead of using electronic devices for a day.",
                TaskPoints = 50,
                AssociatedQuestID = quest12ID,
                QuestContributionAmountOnComplete = 1
            };

            var task13 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 13",
                TaskDescription = "Use fans instead of air-conditioning for a day.",
                TaskPoints = 50,
                AssociatedQuestID = quest13ID,
                QuestContributionAmountOnComplete = 1
            };

            var task14 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 14",
                TaskDescription = "Plant 1 small plant at home.",
                TaskPoints = 75,
                AssociatedQuestID = quest14ID,
                QuestContributionAmountOnComplete = 1
            };

            var task15 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 15",
                TaskDescription = "Pick up 10 pieces of litter from your surroundings.",
                TaskPoints = 75,
                AssociatedQuestID = quest15ID,
                QuestContributionAmountOnComplete = 10
            };

            var task16 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 16",
                TaskDescription = "Use re-usable cutlery for a day.",
                TaskPoints = 75,
                AssociatedQuestID = quest16ID,
                QuestContributionAmountOnComplete = 1
            };

            var task17 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 17",
                TaskDescription = "Bring your own re-usable food containers for a day.",
                TaskPoints = 75,
                AssociatedQuestID = quest17ID,
                QuestContributionAmountOnComplete = 1
            };

            var task18 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 18",
                TaskDescription = "Help water the plants in the school garden.",
                TaskPoints = 75,
                AssociatedQuestID = quest18ID,
                QuestContributionAmountOnComplete = 1
            };

            var task19 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 19",
                TaskDescription = "Create 1 poster on how to reduce waste at school.",
                TaskPoints = 75,
                AssociatedQuestID = quest19ID,
                QuestContributionAmountOnComplete = 1
            };

            var task20 = new Models.Task {
                TaskID = Utilities.GenerateUniqueID(),
                TaskTitle = "Task 20",
                TaskDescription = "Walk or cycle to school for a day.",
                TaskPoints = 75,
                AssociatedQuestID = quest20ID,
                QuestContributionAmountOnComplete = 1
            };

            context.Tasks.AddRange(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14, task15, task16, task17, task18, task19, task20);

            await context.SaveChangesAsync();

            var class1QuestProgress1 = new QuestProgress {
                ClassID = class1.ClassID,
                QuestID = quest1ID,
                Quest = quest1,
                Class = class1,
                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                AmountCompleted = 0,
                Completed = false,
                AssignedTeacherID = teacher1.TeacherID,
                AssignedTeacher = teacher1
            };

            var class1QuestProgress2 = new QuestProgress {
                ClassID = class1.ClassID,
                QuestID = quest2ID,
                Quest = quest2,
                Class = class1,
                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                AmountCompleted = 0,
                Completed = false,
                AssignedTeacherID = teacher1.TeacherID,
                AssignedTeacher = teacher1
            };

            var class1QuestProgress3 = new QuestProgress {
                ClassID = class1.ClassID,
                QuestID = quest3ID,
                Quest = quest3,
                Class = class1,
                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                AmountCompleted = 0,
                Completed = false,
                AssignedTeacherID = teacher1.TeacherID,
                AssignedTeacher = teacher1
            };

            var class2QuestProgress1 = new QuestProgress {
                ClassID = class2.ClassID,
                QuestID = quest4ID,
                Quest = quest4,
                Class = class2,
                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                AmountCompleted = 0,
                Completed = false,
                AssignedTeacherID = teacher1.TeacherID,
                AssignedTeacher = teacher1
            };

            var class2QuestProgress2 = new QuestProgress {
                ClassID = class2.ClassID,
                QuestID = quest5ID,
                Quest = quest5,
                Class = class2,
                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                AmountCompleted = 0,
                Completed = false,
                AssignedTeacherID = teacher1.TeacherID,
                AssignedTeacher = teacher1
            };

            var class2QuestProgress3 = new QuestProgress {
                ClassID = class2.ClassID,
                QuestID = quest6ID,
                Quest = quest6,
                Class = class2,
                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                AmountCompleted = 0,
                Completed = false,
                AssignedTeacherID = teacher1.TeacherID,
                AssignedTeacher = teacher1
            };

            context.QuestProgresses.AddRange(class1QuestProgress1, class1QuestProgress2, class1QuestProgress3, class2QuestProgress1, class2QuestProgress2, class2QuestProgress3);
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
                    ContributingStudentID = context.Students.ToList()[i].StudentID,
                    DateCompleted = DateTime.Now.AddDays(i - 7).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                var class2Points = new ClassPoints {
                    ClassID = class2.ClassID,
                    QuestID = context.Quests.ToList()[i].QuestID,
                    ContributingStudentID = context.Students.ToList()[i].StudentID,
                    DateCompleted = DateTime.Now.AddDays(i - 7).ToString("yyyy-MM-dd"),
                    PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                };

                context.ClassPoints.Add(class1Points);
                context.ClassPoints.Add(class2Points);
            }

            var defaultRewards = new List<RewardItem> {
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "Reusable Water Bottle",
                    RewardDescription = "Leak-proof, stainless steel bottle to stay hydrated throughout the day.",
                    RequiredPoints = 250,
                    RewardQuantity = 5,
                    IsAvailable = true,
                    ImageUrl = "reusable_bottle.jpg"
                },
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "Recycled Paper Notebook",
                    RewardDescription = "100% recycled paper notebook for taking notes sustainably.",
                    RequiredPoints = 180,
                    RewardQuantity = 7,
                    IsAvailable = true,
                    ImageUrl = "recycled_notebook.jpg"
                },
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "Solar-Powered Power Bank",
                    RewardDescription = "Charge your devices on the go with a solar-powered battery bank.",
                    RequiredPoints = 600,
                    RewardQuantity = 3,
                    IsAvailable = true,
                    ImageUrl = "solar_power_bank.jpg"
                },
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "Bamboo Pen Set",
                    RewardDescription = "Eco-friendly bamboo pens for smooth writing.",
                    RequiredPoints = 120,
                    RewardQuantity = 10,
                    IsAvailable = true,
                    ImageUrl = "bamboo_pens.jpg"
                },
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "Reusable Coffee Cup",
                    RewardDescription = "Insulated, reusable cup for hot drinks on campus.",
                    RequiredPoints = 300,
                    RewardQuantity = 6,
                    IsAvailable = true,
                    ImageUrl = "reusable_coffee_cup.jpg"
                },
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "Eco-Friendly Backpack",
                    RewardDescription = "Durable backpack made from recycled materials.",
                    RequiredPoints = 800,
                    RewardQuantity = 2,
                    IsAvailable = true,
                    ImageUrl = "eco_backpack.jpg"
                },
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "LED Desk Lamp (Energy Efficient)",
                    RewardDescription = "Rechargeable LED lamp for late-night study sessions.",
                    RequiredPoints = 500,
                    RewardQuantity = 4,
                    IsAvailable = true,
                    ImageUrl = "led_desk_lamp.jpg"
                },
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "Plant-Based Highlighters",
                    RewardDescription = "Non-toxic, refillable highlighters made from natural materials.",
                    RequiredPoints = 150,
                    RewardQuantity = 8,
                    IsAvailable = true,
                    ImageUrl = "plant_highlighters.jpg"
                },
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "Reusable Sticky Notes",
                    RewardDescription = "Dry-erase sticky notes that can be reused, reducing paper waste.",
                    RequiredPoints = 200,
                    RewardQuantity = 7,
                    IsAvailable = true,
                    ImageUrl = "reusable_sticky_notes.jpg"
                },
                new RewardItem {
                    RewardID = Utilities.GenerateUniqueID(),
                    RewardTitle = "Organic Cotton Tote Bag",
                    RewardDescription = "Lightweight, reusable bag for carrying books or groceries.",
                    RequiredPoints = 160,
                    RewardQuantity = 9,
                    IsAvailable = true,
                    ImageUrl = "cotton_tote_bag.jpg"
                }
            };

            foreach (var reward in defaultRewards) {
                try {
                    var filePath = Path.Combine("wwwroot", "Assets", "Rewards", Path.GetFileName(reward.ImageUrl ?? string.Empty));
                    
                    if (File.Exists(filePath)) {
                        using var stream = new FileStream(filePath, FileMode.Open);
                        
                        var formFile = new FormFile(stream, 0, stream.Length, "", Path.GetFileName(filePath)) {
                            Headers = new HeaderDictionary(),
                            ContentType = "image/jpeg"
                        };

                        var uploadResult = await AssetsManager.UploadFileAsync(formFile);

                        if (uploadResult.StartsWith("SUCCESS")) {
                            reward.ImageUrl = await AssetsManager.GetFileUrlAsync(Path.GetFileName(filePath));
                            reward.ImageUrl = reward.ImageUrl.Substring("SUCCESS: ".Length).Trim();
                        }
                    }
                } catch (Exception ex) {    
                    throw new Exception("Error uploading default reward image: " + ex.Message);
                }
            }

            context.RewardItems.AddRange(defaultRewards);
            await context.SaveChangesAsync();

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