using System.Net;
using System.Text;
using Backend.Filters;
using Backend.Models;
using Backend.Services;
using DotNetEnv;
using EcoPilotApp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Task = System.Threading.Tasks.Task;

namespace Backend {
    class SuperuserScript {
        private readonly MyDbContext _context;
        private readonly IConfiguration _config;
        private static readonly Random random = new Random();
        private readonly MSAuth _msAuth;

        public SuperuserScript(MyDbContext context, IConfiguration config, MSAuth msAuthService) {
            _context = context;
            _config = config;
            _msAuth = msAuthService;
        }

        public async Task Run() {
            Console.WriteLine("");
            Console.Write("Username: ");
            string superuserUsername = Console.ReadLine() ?? "";
            Console.Write("Password: ");
            string superuserPassword = Console.ReadLine() ?? "";
            Console.Write("PIN: ");
            string superuserPIN = Console.ReadLine() ?? "";

            if (superuserUsername != _config["SUPERUSER_USERNAME"] || superuserPassword != _config["SUPERUSER_PASSWORD"] || superuserPIN != _config["SUPERUSER_PIN"]) {
                Console.WriteLine("");
                Console.WriteLine("ACCESS UNAUTHORISED: Invalid superuser credentials.");
                Console.WriteLine("");

                for (int i = 6; i > 0; i--) {
                    for (int j = 0; j < 10; j++) {
                        if (Console.KeyAvailable) {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Enter) {
                                Console.WriteLine("");
                                Console.WriteLine("");
                                Console.WriteLine("Console terminated. Goodbye!");
                                Environment.Exit(0);
                            }
                        }
                        Thread.Sleep(100);
                    }
                    Console.Write($"\r[Press ENTER to CANCEL] Switching to STANDARD SERVER MODE in {i - 1} seconds...   ");
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("SERVER MODE: STANDARD");
                Console.WriteLine();
                Console.Write($"Server running on {Environment.GetEnvironmentVariable("HTTPS_URL")}/swagger/index.html");
                return;
            } else {
                Console.WriteLine("");
                Console.WriteLine("ACCESS AUTHORISED: Launching superuser script...");
                bool running = true;
                while (running) {
                    Console.WriteLine();
                    Console.WriteLine("---------------------------------Welcome to the Recyclify System Superuser Console----------------------------------------------");
                    Console.WriteLine("Populate CloudSQL Database now handles all population logic. Please run this command first before other actions.");
                    Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("1. Create new account");
                    Console.WriteLine("2. Delete existing account");
                    Console.WriteLine("3. Lock / Unlock System");
                    Console.WriteLine("4. Toggle services");
                    Console.WriteLine("5. Clear Firebase Cloud Storage");
                    Console.WriteLine("6. Wipe CloudSQL Database");
                    Console.WriteLine("7. Populate CloudSQL Database");
                    Console.WriteLine("8. Exit Console");

                    Console.WriteLine();
                    Console.Write("Enter action: ");

                    if (!int.TryParse(Console.ReadLine(), out int action)) {
                        Console.WriteLine("");
                        Console.WriteLine("ERROR: Please enter a valid integer.");
                        continue;
                    }

                    switch (action) {
                        case 1:
                            await CreateAccount();
                            break;
                        case 2:
                            await DeleteAccount();
                            break;
                        case 3:
                            await ToggleLockSystem();
                            break;
                        case 4:
                            await ToggleServices();
                            break;
                        case 5:
                            await ClearFirebaseCloudStorage();
                            break;
                        case 6:
                            await WipeDatabase();
                            break;
                        case 7:
                            await PopulateDatabase();
                            break;
                        case 8:
                            Console.WriteLine("");
                            Console.WriteLine("Exiting superuser script...");
                            Console.WriteLine("");
                            Console.WriteLine("SERVER MODE: STANDARD");
                            Console.WriteLine("");
                            Console.Write($"Server running on {Environment.GetEnvironmentVariable("HTTPS_URL")}/swagger/index.html");
                            running = false;
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("ERROR: Please enter a valid integer from 1-8.");
                            break;
                    }
                }
            }
        }

        private async Task CreateAccount() {
            Console.WriteLine("");
            Console.WriteLine("1. Admin");
            Console.WriteLine("2. Teacher");
            Console.WriteLine("3. Parent");
            Console.WriteLine("4. Student");
            Console.WriteLine("5. Exit");

            Console.WriteLine();

            Console.Write("Enter account type: ");
            if (!int.TryParse(Console.ReadLine(), out int accountType)) {
                Console.WriteLine("");
                Console.WriteLine("ERROR: Please enter a valid integer.");
                return;
            }

            switch (accountType) {
                case 1:
                    Console.WriteLine("");
                    Console.Write("Admin First Name: ");
                    string adminFName = Console.ReadLine() ?? "";
                    Console.Write("Admin Last Name: ");
                    string adminLName = Console.ReadLine() ?? "";
                    Console.Write("Admin Email: ");
                    string adminEmail = Console.ReadLine() ?? "";
                    Console.Write("Admin Password: ");
                    string adminPassword = Console.ReadLine() ?? "";
                    Console.Write("Admin Contact Number: ");
                    string adminContactNumber = Console.ReadLine() ?? "";
            
                    var adminKvp = new List<Dictionary<string, object>> {
                        new Dictionary<string, object> {
                            { "Name", adminFName + " " + adminLName },
                            { "FName", adminFName },
                            { "LName", adminLName },
                            { "Email", adminEmail },
                            { "Password", adminPassword },
                            { "ContactNumber", adminContactNumber },
                            { "UserRole", "admin" },
                            { "Avatar", "" },
                            { "EmailVerified", false }
                        }
                    };

                    try {
                        await DatabaseManager.CreateUserRecords(_context, "admin", adminKvp);
                        Console.WriteLine("");
                        Console.WriteLine("Admin Account created successfully.");
                        Console.WriteLine("-------------------------------------------");
                        Console.WriteLine("Admin Username: " + adminKvp[0]["Email"]);
                        Console.WriteLine("Admin Password: " + adminKvp[0]["Password"]);
                        Console.WriteLine("-------------------------------------------");
                    } catch (Exception ex) {
                        Console.WriteLine("");
                        Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                    }

                    break;
                case 2:
                    Console.WriteLine("");
                    Console.Write("Teacher First Name: ");
                    string teacherFName = Console.ReadLine() ?? "";
                    Console.Write("Teacher Last Name: ");
                    string teacherLName = Console.ReadLine() ?? "";
                    Console.Write("Teacher Email: ");
                    string teacherEmail = Console.ReadLine() ?? "";
                    Console.Write("Teacher Password: ");
                    string teacherPassword = Console.ReadLine() ?? "";
                    Console.Write("Teacher Contact Number: ");
                    string teacherContactNumber = Console.ReadLine() ?? "";
                    Console.Write("Associated Class Name: ");

                    if (!int.TryParse(Console.ReadLine(), out int className)) {
                        Console.WriteLine("");
                        Console.WriteLine("ERROR: Please enter a valid integer for Class Name.");
                        return;
                    }

                    Console.Write("Class Description: ");
                    string classDescription = Console.ReadLine() ?? "";

                    var teacherKvp = new List<Dictionary<string, object>> {
                        new Dictionary<string, object> {
                            { "Id", Utilities.GenerateUniqueID() },
                            { "Name", teacherFName + " " + teacherLName },
                            { "FName", teacherFName },
                            { "LName", teacherLName },
                            { "Email", teacherEmail },
                            { "Password", teacherPassword },
                            { "ContactNumber", teacherContactNumber },
                            { "UserRole", "teacher" },
                            { "Avatar", "" },
                            { "EmailVerified", false }
                        }
                    };

                    try {
                        await DatabaseManager.CreateUserRecords(_context, "teacher", teacherKvp);

                        var teacherClassID = Utilities.GenerateUniqueID();
                        var teacherClass = new Class {
                            ClassID = teacherClassID,
                            ClassName = className,
                            ClassDescription = classDescription,
                            ClassPoints = Utilities.GenerateRandomInt(500, 1000),
                            TeacherID = teacherKvp[0]["Id"].ToString() ?? "",
                            Teacher = _context.Teachers.Find(teacherKvp[0]["Id"].ToString()) ?? throw new Exception("ERROR: Teacher not found."),
                            WeeklyClassPoints = new List<WeeklyClassPoints>(),
                            JoinCode = Utilities.GenerateRandomInt(100000, 999999)
                        };

                        teacherClass.WeeklyClassPoints = new List<WeeklyClassPoints> {
                            new WeeklyClassPoints {
                                ClassID = teacherClassID,
                                Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                                PointsGained = Utilities.GenerateRandomInt(500, 1000),
                                Class = teacherClass
                            }
                        };

                        _context.Classes.Add(teacherClass);

                        var questList = _context.Quests.ToList();

                        if (questList.Count < 3) {
                            throw new Exception("ERROR: Not enough quests in database. Please Populate CloudSQL Database first.");
                        }

                        for (int i = 0; i < 3; i++) {
                            var randomQuest = questList[random.Next(questList.Count)];
                            var classQuestProgress = new QuestProgress {
                                ClassID = teacherClassID,
                                QuestID = randomQuest.QuestID,
                                Quest = randomQuest,
                                Class = teacherClass,
                                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                                AmountCompleted = 0,
                                Completed = false,
                                AssignedTeacherID = teacherKvp[0]["Id"].ToString() ?? "",
                                AssignedTeacher = _context.Teachers.Find(teacherKvp[0]["Id"].ToString()) ?? throw new Exception("Teacher not found.")
                            };

                            _context.QuestProgresses.Add(classQuestProgress);
                        }

                        await _context.SaveChangesAsync();

                        Console.WriteLine("");
                        Console.WriteLine("Teacher Account created successfully.");
                        Console.WriteLine("-------------------------------------------");
                        Console.WriteLine("Teacher Username: " + teacherKvp[0]["Email"]);
                        Console.WriteLine("Teacher Password: " + teacherKvp[0]["Password"]);
                        Console.WriteLine("----------------");
                        Console.WriteLine("Associated Class ID: " + teacherClass.ClassID);
                        Console.WriteLine("Associated Class Name: " + teacherClass.ClassName);
                        Console.WriteLine("-------------------------------------------");
                    } catch (Exception ex) {
                        Console.WriteLine("");
                        Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                    }

                    break;
                case 3:
                    Console.WriteLine("");
                    Console.Write("Parent First Name: ");
                    string parentFName = Console.ReadLine() ?? "";
                    Console.Write("Parent Last Name: ");
                    string parentLName = Console.ReadLine() ?? "";
                    Console.Write("Parent Email: ");
                    string parentEmail = Console.ReadLine() ?? "";
                    Console.Write("Parent Password: ");
                    string parentPassword = Console.ReadLine() ?? "";
                    Console.Write("Parent Contact Number: ");
                    string parentContactNumber = Console.ReadLine() ?? "";

                    var parentKvp = new List<Dictionary<string, object>> {
                        new Dictionary<string, object> {
                            { "Id", Utilities.GenerateUniqueID() },
                            { "Name", parentFName + " " + parentLName },
                            { "FName", parentFName },
                            { "LName", parentLName },
                            { "Email", parentEmail },
                            { "Password", parentPassword },
                            { "ContactNumber", parentContactNumber },
                            { "UserRole", "parent" },
                            { "Avatar", "" },
                            { "EmailVerified", false }
                        }
                    };

                    try {
                        await DatabaseManager.CreateUserRecords(_context, "parent", parentKvp);
                        Console.WriteLine("");
                        Console.WriteLine("Parent Account created successfully.");
                        Console.WriteLine("-------------------------------------------");
                        Console.WriteLine("Parent Username: " + parentKvp[0]["Email"]);
                        Console.WriteLine("Parent Password: " + parentKvp[0]["Password"]);
                        Console.WriteLine("-------------------------------------------");
                    } catch (Exception ex) {
                        Console.WriteLine("");
                        Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                    }

                    break;
                case 4:
                    Console.WriteLine("");
                    Console.Write("Student First Name: ");
                    string studentFName = Console.ReadLine() ?? "";
                    Console.Write("Student Last Name: ");
                    string studentLName = Console.ReadLine() ?? "";
                    Console.Write("Student Email: ");
                    string studentEmail = Console.ReadLine() ?? "";
                    Console.Write("Student Password: ");
                    string studentPassword = Console.ReadLine() ?? "";
                    Console.Write("Student Contact Number: ");
                    string studentContactNumber = Console.ReadLine() ?? "";

                    var studentKvp = new List<Dictionary<string, object>> {
                        new Dictionary<string, object> {
                            { "Id", Utilities.GenerateUniqueID() },
                            { "Name", studentFName + " " + studentLName },
                            { "FName", studentFName },
                            { "LName", studentLName },
                            { "Email", studentEmail },
                            { "Password", studentPassword },
                            { "ContactNumber", studentContactNumber },
                            { "UserRole", "student" },
                            { "Avatar", "" },
                            { "EmailVerified", false }
                        }
                    };

                    try {
                        await DatabaseManager.CreateUserRecords(_context, "student", studentKvp);

                        for (int i = 0; i < 10; i++) {
                            var studentPoints = new StudentPoints {
                                StudentID = studentKvp[0]["Id"].ToString() ?? "",
                                TaskID = _context.Tasks.ToList()[i].TaskID,
                                DateCompleted = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                                PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                            };

                            _context.StudentPoints.Add(studentPoints);
                        }

                        await _context.SaveChangesAsync();

                        Console.WriteLine("");
                        Console.WriteLine("Student Account created successfully.");
                        Console.WriteLine("-------------------------------------------");
                        Console.WriteLine("Student Username: " + studentKvp[0]["Email"]);
                        Console.WriteLine("Student Password: " + studentKvp[0]["Password"]);
                        Console.WriteLine("-------------------------------------------");
                    } catch (Exception ex) {
                        Console.WriteLine("");
                        Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                    }

                    break;
                case 5:
                    Console.WriteLine("");
                    Console.WriteLine("Exiting Account Creation Mode...");
                    break;
                default:
                    Console.WriteLine("");
                    Console.WriteLine("ERROR: Please enter a valid integer from 1-5.");
                    break;
            }
        }

        private async Task DeleteAccount() {
            Console.WriteLine("");
            Console.Write("Enter UserID of account to be deleted: ");
            string userID = Console.ReadLine() ?? "";
            try {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userID) ?? throw new Exception("ERROR: No such user.");
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                Console.WriteLine("");
                Console.WriteLine("SUCCESS: Account deleted.");
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }
        }

        private async Task ToggleLockSystem() {
            try {
                var systemLocked = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "SYSTEM_LOCKED") ?? throw new Exception("ERROR: SYSTEM_LOCKED environment variable not found.");
                Console.WriteLine("");
                Console.WriteLine("System Lock Status: " + systemLocked.Value);
                Console.WriteLine("");
                Console.WriteLine("1. Lock System");
                Console.WriteLine("2. Unlock System");
                Console.WriteLine("3. Exit");

                Console.WriteLine();
                Console.Write("Enter action: ");
                if (!int.TryParse(Console.ReadLine(), out int action)) {
                    Console.WriteLine("");
                    Console.WriteLine("ERROR: Please enter a valid integer.");
                    return;
                }

                switch (action) {
                    case 1:
                        systemLocked.Value = "true";
                        await _context.SaveChangesAsync();
                        Console.WriteLine("");
                        Console.WriteLine("SUCCESS: System locked.");
                        break;
                    case 2:
                        systemLocked.Value = "false";
                        await _context.SaveChangesAsync();
                        Console.WriteLine("");
                        Console.WriteLine("SUCCESS: System unlocked.");
                        break;
                    case 3:
                        Console.WriteLine("");
                        Console.WriteLine("Exiting System Lock Mode...");
                        break;
                    default:
                        Console.WriteLine("");
                        Console.WriteLine("ERROR: Please enter a valid integer from 1-3.");
                        break;
                } 

                return;
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");

                return;
            }
        }

        private async Task ToggleServices() {
            while (true) {
                var compVisionEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "COMPVISION_ENABLED")
                    ?? throw new Exception("ERROR: COMPVISION_ENABLED environment variable not found.");
                var emailerEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "EMAILER_ENABLED")
                    ?? throw new Exception("ERROR: EMAILER_ENABLED environment variable not found.");
                var openAIChatServiceEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "OPENAI_CHAT_SERVICE_ENABLED")
                    ?? throw new Exception("ERROR: OPENAI_CHAT_SERVICE_ENABLED environment variable not found.");
                var smsServiceEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "SMS_ENABLED")
                    ?? throw new Exception("ERROR: SMS_ENABLED environment variable not found.");
                var msAuthEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "MSAuthEnabled")
                    ?? throw new Exception("ERROR: MSAuthEnabled environment variable not found.");

                Console.WriteLine("");
                Console.WriteLine("-----Services-----");
                Console.WriteLine("1. CompVision - " + (compVisionEnabled.Value == "true" ? "Enabled" : "Disabled"));
                Console.WriteLine("2. Emailer - " + (emailerEnabled.Value == "true" ? "Enabled" : "Disabled"));
                Console.WriteLine("3. OpenAIChatService - " + (openAIChatServiceEnabled.Value == "true" ? "Enabled" : "Disabled"));
                Console.WriteLine("4. SmsService - " + (smsServiceEnabled.Value == "true" ? "Enabled" : "Disabled"));
                Console.WriteLine("5. Microsoft Auth - " + (msAuthEnabled.Value == "true" ? "Enabled" : "Disabled"));
                Console.WriteLine("------WARNING------");
                Console.WriteLine("6. DISABLE ALL SERVICES");
                Console.WriteLine("7. ENABLE ALL SERVICES");
                Console.WriteLine("8. Exit");
                Console.WriteLine();
                Console.Write("Select service to toggle: ");

                if (!int.TryParse(Console.ReadLine(), out int service)) {
                    Console.WriteLine("");
                    Console.WriteLine("ERROR: Please enter a valid integer.");
                    continue;
                }

                try {
                    switch (service) {
                        case 1:
                            compVisionEnabled.Value = compVisionEnabled.Value == "true" ? "false" : "true";
                            await _context.SaveChangesAsync();
                            Console.WriteLine("");
                            Console.WriteLine("SUCCESS: CompVision service " + (compVisionEnabled.Value == "true" ? "ENABLED." : "DISABLED."));
                            break;
                        case 2:
                            emailerEnabled.Value = emailerEnabled.Value == "true" ? "false" : "true";
                            await _context.SaveChangesAsync();
                            Console.WriteLine("");
                            Console.WriteLine("SUCCESS: Emailer service " + (emailerEnabled.Value == "true" ? "ENABLED." : "DISABLED."));
                            break;
                        case 3:
                            openAIChatServiceEnabled.Value = openAIChatServiceEnabled.Value == "true" ? "false" : "true";
                            await _context.SaveChangesAsync();
                            Console.WriteLine("");
                            Console.WriteLine("SUCCESS: OpenAIChatService service " + (openAIChatServiceEnabled.Value == "true" ? "ENABLED." : "DISABLED."));
                            break;
                        case 4:
                            smsServiceEnabled.Value = smsServiceEnabled.Value == "true" ? "false" : "true";
                            await _context.SaveChangesAsync();
                            Console.WriteLine("");
                            Console.WriteLine("SUCCESS: SmsService service " + (smsServiceEnabled.Value == "true" ? "ENABLED." : "DISABLED."));
                            break;
                        case 5:
                            msAuthEnabled.Value = msAuthEnabled.Value == "true" ? "false" : "true";
                            await _context.SaveChangesAsync();
                            Console.WriteLine("");
                            Console.WriteLine("SUCCESS: Microsoft Auth service " + (msAuthEnabled.Value == "true" ? "ENABLED." : "DISABLED."));
                            break;
                        case 6:
                            Console.WriteLine("");

                            for (int i = 6; i > 0; i--) {
                                for (int j = 0; j < 10; j++) {
                                    if (Console.KeyAvailable) {
                                        var key = Console.ReadKey(true);
                                        if (key.Key == ConsoleKey.Enter) {
                                            Console.WriteLine("");
                                            Console.WriteLine("");
                                            Console.WriteLine("[ABORT] - Process terminated.");
                                            return;
                                        }
                                    }
                                    Thread.Sleep(100);
                                }
                                Console.Write($"\r[Press ENTER to CANCEL] DISABLING ALL SERVICES in {i - 1} seconds...");
                            }

                            compVisionEnabled.Value = "false";
                            emailerEnabled.Value = "false";
                            openAIChatServiceEnabled.Value = "false";
                            smsServiceEnabled.Value = "false";
                            msAuthEnabled.Value = "false";
                            await _context.SaveChangesAsync();
                            Console.WriteLine("");
                            Console.WriteLine("");
                            Console.WriteLine("SUCCESS: ALL SERVICES DISABLED.");
                            break;
                        case 7:
                            Console.WriteLine("");

                            for (int i = 6; i > 0; i--) {
                                for (int j = 0; j < 10; j++) {
                                    if (Console.KeyAvailable) {
                                        var key = Console.ReadKey(true);
                                        if (key.Key == ConsoleKey.Enter) {
                                            Console.WriteLine("");
                                            Console.WriteLine("");
                                            Console.WriteLine("[ABORT] - Process terminated.");
                                            return;
                                        }
                                    }
                                    Thread.Sleep(100);
                                }
                                Console.Write($"\r[Press ENTER to CANCEL] ENABLING ALL SERVICES in {i - 1} seconds...");
                            }

                            compVisionEnabled.Value = "true";
                            emailerEnabled.Value = "true";
                            openAIChatServiceEnabled.Value = "true";
                            smsServiceEnabled.Value = "true";
                            msAuthEnabled.Value = "true";
                            await _context.SaveChangesAsync();
                            Console.WriteLine("");
                            Console.WriteLine("");
                            Console.WriteLine("SUCCESS: ALL SERVICES ENABLED.");
                            break;
                        case 8:
                            Console.WriteLine("");
                            Console.WriteLine("Exiting Service Toggle Mode...");
                            return;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("ERROR: Please enter a valid integer from 1-8.");
                            break;
                    }
                } catch (Exception ex) {
                    Console.WriteLine("");
                    Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                }
            }
        }

        private async Task ClearFirebaseCloudStorage() {
            Console.WriteLine("");

            for (int i = 6; i > 0; i--) {
                for (int j = 0; j < 10; j++) {
                    if (Console.KeyAvailable) {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Enter) {
                            Console.WriteLine("");
                            Console.WriteLine("");
                            Console.WriteLine("[ABORT] - Process terminated.");
                            return;
                        }
                    }
                    Thread.Sleep(100);
                }
                Console.Write($"\r[Press ENTER to CANCEL] Wiping Firebase Cloud Storage in {i - 1} seconds...   ");
            }

            try {
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("Database Wipe in progress. This may take a while...");

                await AssetsManager.ClearFirebaseCloudStorage();

                Console.WriteLine("");
                Console.WriteLine("SUCCESS: FIREBASE CLOUD STORAGE WIPED.");

                return;
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");

                return;
            }
        }

        private async Task WipeDatabase() {
            try {
                Console.WriteLine("");
                Console.Write("Wiping Database...");

                _context.Admins.RemoveRange(_context.Admins);
                _context.Classes.RemoveRange(_context.Classes);
                _context.ClassPoints.RemoveRange(_context.ClassPoints);
                _context.ClassStudents.RemoveRange(_context.ClassStudents);
                _context.ContactForms.RemoveRange(_context.ContactForms);
                _context.DailyStudentPoints.RemoveRange(_context.DailyStudentPoints);
                _context.Inboxes.RemoveRange(_context.Inboxes);
                _context.Parents.RemoveRange(_context.Parents);
                _context.Quests.RemoveRange(_context.Quests);
                _context.QuestProgresses.RemoveRange(_context.QuestProgresses);
                _context.Redemptions.RemoveRange(_context.Redemptions);
                _context.RewardItems.RemoveRange(_context.RewardItems);
                _context.Students.RemoveRange(_context.Students);
                _context.StudentPoints.RemoveRange(_context.StudentPoints);
                _context.Tasks.RemoveRange(_context.Tasks);
                _context.TaskProgresses.RemoveRange(_context.TaskProgresses);
                _context.Teachers.RemoveRange(_context.Teachers);
                _context.Users.RemoveRange(_context.Users);
                _context.WeeklyClassPoints.RemoveRange(_context.WeeklyClassPoints);
                _context.EnvironmentConfigs.RemoveRange(_context.EnvironmentConfigs);
                _context.Events.RemoveRange(_context.Events);

                await _context.SaveChangesAsync();

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("SUCCESS: CLOUDSQL DATABASE WIPED.");

                return;
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");

                return;
            }
        }

        private async Task PopulateDatabase() {
            try {
                await WipeDatabase();
                await PopulateCloudConfigs();
                await PopulateTasksAndQuests();
                await PopulateRewardItems();
                await PopulateEvents();
                await PopulatePresentationUsers();
                await PopulateStudents();

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("SUCCESS: Database populated.");

                return;
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");

                return;
            }
        }

        private async Task PopulateCloudConfigs() {
            Console.WriteLine("");
            Console.Write("Populating Cloud Configs...");
            try {
                var environmentVariables = Bootcheck.RetrieveEnvironmentVariables();
                _context.EnvironmentConfigs.RemoveRange(_context.EnvironmentConfigs);

                foreach (var envVar in environmentVariables) {
                    var value = Environment.GetEnvironmentVariable(envVar) ?? "Not set";
                    var config = new EnvironmentConfig {
                        Name = envVar,
                        Value = value
                    };
                    
                    _context.EnvironmentConfigs.Add(config);
                }
                await _context.SaveChangesAsync();

                return;
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                return;
            }
        }

        private async Task PopulateTasksAndQuests() {
            Console.WriteLine("");
            Console.Write("Populating Tasks and Quests...");
            try {
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

                _context.Quests.AddRange(quest1, quest2, quest3, quest4, quest5, quest6, quest7, quest8, quest9, quest10, quest11, quest12, quest13, quest14, quest15, quest16, quest17, quest18, quest19, quest20);

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

                _context.Tasks.AddRange(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14, task15, task16, task17, task18, task19, task20);

                await _context.SaveChangesAsync();

                return;
            } catch(Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");

                return;
            }
        }

        private async Task PopulateRewardItems() {
            Console.WriteLine("");
            Console.Write("Populating Reward Items. This may take a while...");
            try {
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
                        Console.WriteLine("");
                        Console.WriteLine($"ERROR: {ex.Message}");
                    }
                }

                _context.RewardItems.AddRange(defaultRewards);
                await _context.SaveChangesAsync();

                return;
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");

                return;
            }
        }

        private async Task PopulateEvents() {
            Console.WriteLine("");
            Console.Write("Populating Events...");
            try {
                var event1 = new Event {
                    Id = Utilities.GenerateUniqueID(),
                    Title = "Recycling Awareness Campaign",
                    Description = "Join us for a campaign to raise awareness about recycling.",
                    EventDateTime = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"),
                    ImageUrl = "",
                    PostedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var event2 = new Event {
                    Id = Utilities.GenerateUniqueID(),
                    Title = "Eco-Friendly Workshop",
                    Description = "Learn how to make eco-friendly products at home.",
                    EventDateTime = DateTime.Now.AddDays(14).ToString("yyyy-MM-dd"),
                    ImageUrl = "",
                    PostedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var event3 = new Event {
                    Id = Utilities.GenerateUniqueID(),
                    Title = "Energy Conservation Talk",
                    Description = "Join us for a talk on energy conservation and sustainability.",
                    EventDateTime = DateTime.Now.AddDays(21).ToString("yyyy-MM-dd"),
                    ImageUrl = "",
                    PostedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _context.Events.AddRange(event1, event2, event3);
                await _context.SaveChangesAsync();

                return;
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");

                return;
            }
        }

        private async Task PopulatePresentationUsers() {
            Console.WriteLine("");
            Console.Write("Populating Presentation Users...");
            try {
                _context.Users.RemoveRange(_context.Users);
                _context.Admins.RemoveRange(_context.Admins);
                _context.Teachers.RemoveRange(_context.Teachers);
                _context.Students.RemoveRange(_context.Students);

                await _context.SaveChangesAsync();

                await DatabaseManager.CreateUserRecords(_context, "admin", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Name", "3xpect1916" },
                        { "FName", "Nicholas" },
                        { "LName", "Chew" },
                        { "Email", "3xpect1916@gmail.com" },
                        { "Password", Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD") ?? throw new Exception("ERROR: DEFAULT_ADMIN_PASSWORD environment variable not found.") },
                        { "ContactNumber", "88133912" },
                        { "UserRole", "admin" },
                        { "Avatar", "" }
                    }
                });

                var teacherID = Utilities.GenerateUniqueID();
                var teacherClassID = Utilities.GenerateUniqueID();
                await DatabaseManager.CreateUserRecords(_context, "teacher", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", teacherID },
                        { "Name", "lincolnlim267" },
                        { "FName", "Lincoln" },
                        { "LName", "Lim" },
                        { "Email", "lincolnlim267@gmail.com" },
                        { "Password", Environment.GetEnvironmentVariable("DEFAULT_TEACHER_PASSWORD") ?? throw new Exception("ERROR: DEFAULT_TEACHER_PASSWORD environment variable not found.") },
                        { "ContactNumber", "80136850" },
                        { "UserRole", "teacher" },
                        { "Avatar", "" }
                    }
                });

                var teacherClass = new Class {
                    ClassID = teacherClassID,
                    ClassName = 101,
                    ClassDescription = "Class 101",
                    ClassPoints = Utilities.GenerateRandomInt(500, 1000),
                    TeacherID = teacherID,
                    Teacher = _context.Teachers.Find(teacherID) ?? throw new Exception("ERROR: Teacher not found."),
                    WeeklyClassPoints = new List<WeeklyClassPoints>(),
                    JoinCode = Utilities.GenerateRandomInt(100000, 999999)
                };

                teacherClass.WeeklyClassPoints = new List<WeeklyClassPoints> {
                    new WeeklyClassPoints {
                        ClassID = teacherClassID,
                        Date = DateTime.Now.AddDays(new Random().Next((DateTime.Now - new DateTime(2020, 1, 1)).Days)),
                        PointsGained = Utilities.GenerateRandomInt(500, 1000),
                        Class = teacherClass
                    }
                };

                await _context.Classes.AddAsync(teacherClass);

                var studentObjPassword = Environment.GetEnvironmentVariable("DEFAULT_STUDENT_PASSWORD") ?? throw new Exception("ERROR: DEFAULT_STUDENT_PASSWORD environment variable not found.");
                var baseUserObj = new User {
                    Id = Utilities.GenerateUniqueID(),
                    Name = "joshu5739yx",
                    FName = "Joshua",
                    LName = "Long",
                    Email = "joshu5739yx@gmail.com",
                    Password = Utilities.HashString(studentObjPassword),
                    ContactNumber = "83880976",
                    UserRole = "student",
                    Avatar = "",
                    EmailVerified = false,
                    PhoneVerified = false
                };

                var specificStudentObj = new Student {
                    UserID = baseUserObj.Id,
                    StudentID = baseUserObj.Id,
                    Streak = 6,
                    League = "Bronze",
                    CurrentPoints = 0,
                    TotalPoints = 0,
                    LastActiveDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")
                };

                for (int i = 0; i < 10; i++) {
                    var addStudentPoints = new StudentPoints {
                        StudentID = baseUserObj.Id,
                        TaskID = _context.Tasks.ToList()[i].TaskID,
                        PointsAwarded = _context.Tasks.ToList()[i].TaskPoints,
                        DateCompleted = DateTime.Now.AddDays(-7).AddDays(i).ToString("yyyy-MM-dd"),
                    };

                    specificStudentObj.CurrentPoints += addStudentPoints.PointsAwarded;
                    specificStudentObj.TotalPoints += addStudentPoints.PointsAwarded;

                    await _context.StudentPoints.AddAsync(addStudentPoints);
                }

                await _context.Users.AddAsync(baseUserObj);
                await _context.Students.AddAsync(specificStudentObj);

                await _context.SaveChangesAsync();

                try {
                    string qrCodeUrl = "";
                    var MsAuthEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "MSAuthEnabled") ?? throw new Exception("ERROR: MSAuthEnabled environment variable not found.");
                    if (MsAuthEnabled.Value == "true") {
                        var user = _context.Users.SingleOrDefault(u => u.Id == baseUserObj.Id) ?? throw new Exception("ERROR: User not found.");

                        if (user.MfaSecret == null || user.MfaSecret == string.Empty) {
                            var newSecretResult = await _msAuth.NewSecret();
                            string secret = newSecretResult.ToString() ?? string.Empty;
                            var enrollResult = await _msAuth.Enroll(user.Email, "Recyclify", secret.Trim());
                            user.MfaSecret = secret;
                            qrCodeUrl = enrollResult?.ToString() ?? string.Empty;

                            _context.SaveChanges();

                            var emailVars = new Dictionary<string, string> {
                                { "username", user.Name },
                                { "qrcode", qrCodeUrl }
                            };

                            var Emailer = new Emailer(_context);
                            await Emailer.SendEmailAsync(user.Email, "Your Authenticator QR Code", "MsAuth", emailVars);
                        } else {
                            var secret = user.MfaSecret;
                            var enrollResult = await _msAuth.Enroll(user.Email, "Recyclify", secret.Trim());
                            qrCodeUrl = enrollResult?.ToString() ?? string.Empty;
                        }
                    }
                } catch (Exception ex) {
                    Console.WriteLine("");
                    Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                }

                var parentID = Utilities.GenerateUniqueID();
                var parentEmail = "tanqianpeng18@gmail.com";
                await DatabaseManager.CreateUserRecords(_context, "parent", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", parentID },
                        { "Name", "tanqianpeng18" },
                        { "FName", "Qian Peng" },
                        { "LName", "Tan" },
                        { "Email", parentEmail },
                        { "Password", Environment.GetEnvironmentVariable("DEFAULT_PARENT_PASSWORD") ?? throw new Exception("ERROR: DEFAULT_PARENT_PASSWORD environment variable not found.") },
                        { "ContactNumber", "87810955" },
                        { "UserRole", "parent" },
                        { "Avatar", "" },
                        { "StudentID", specificStudentObj.StudentID }
                    }
                });

                try {
                    string qrCodeUrl = "";
                    var MsAuthEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "MSAuthEnabled") ?? throw new Exception("ERROR: MSAuthEnabled environment variable not found.");
                    if (MsAuthEnabled.Value == "true") {
                        var user = _context.Users.SingleOrDefault(u => u.Id == parentID) ?? throw new Exception("ERROR: User not found.");

                        if (user.MfaSecret == null || user.MfaSecret == string.Empty) {
                            var newSecretResult = await _msAuth.NewSecret();
                            string secret = newSecretResult.ToString() ?? string.Empty;
                            var enrollResult = await _msAuth.Enroll(user.Email, "Recyclify", secret.Trim());
                            user.MfaSecret = secret;
                            qrCodeUrl = enrollResult?.ToString() ?? string.Empty;

                            _context.SaveChanges();

                            var emailVars = new Dictionary<string, string> {
                                { "username", user.Name },
                                { "qrcode", qrCodeUrl }
                            };

                            var Emailer = new Emailer(_context);
                            await Emailer.SendEmailAsync(parentEmail, "Your Authenticator QR Code", "MsAuth", emailVars);
                        } else {
                            var secret = user.MfaSecret;
                            var enrollResult = await _msAuth.Enroll(user.Email, "Recyclify", secret.Trim());
                            qrCodeUrl = enrollResult?.ToString() ?? string.Empty;
                        }
                    }
                } catch (Exception ex) {
                    Console.WriteLine("");
                    Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                }

                return;
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");

                return;
            }
        }

        private async Task PopulateStudents() {
            var existingTeachers = _context.Teachers.ToList();
            var classes = _context.Classes.ToList();
            if (existingTeachers.Count == 0) {
                Console.WriteLine("");
                Console.WriteLine("ERROR: Please create a Teacher Account first.");
                return;
            }
            if (classes.Count == 0) {
                Console.WriteLine("");
                Console.WriteLine("ERROR: No Classes found. Wipe Database and try creating a new Teacher Account again.");
                return;
            }

            Console.WriteLine("");
            Console.Write("Populating Students...");

            try {
                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "lanang" },
                        { "FName", "Lana" },
                        { "LName", "Ng" },
                        { "Email", "000000p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "kategibson" },
                        { "FName", "Kate" },
                        { "LName", "Gibson" },
                        { "Email", "000001p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "peterparker" },
                        { "FName", "Peter" },
                        { "LName", "Parker" },
                        { "Email", "000002p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "ethancarter" },
                        { "FName", "Ethan" },
                        { "LName", "Carter" },
                        { "Email", "000003p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "oliviabennett" },
                        { "FName", "Olivia" },
                        { "LName", "Bennett" },
                        { "Email", "000004p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "noahmitchell" },
                        { "FName", "Noah" },
                        { "LName", "Mitchell" },
                        { "Email", "000005p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "emmarobinson" },
                        { "FName", "Emma" },
                        { "LName", "Robinson" },
                        { "Email", "000006p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "liamturner" },
                        { "FName", "Liam" },
                        { "LName", "Turner" },
                        { "Email", "000007p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "avaparker" },
                        { "FName", "Ava" },
                        { "LName", "Parker" },
                        { "Email", "000008p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                await DatabaseManager.CreateUserRecords(_context, "student", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        { "Id", Utilities.GenerateUniqueID() },
                        { "Name", "sophiaramirez" },
                        { "FName", "Sophia" },
                        { "LName", "Ramirez" },
                        { "Email", "000009p@mymail.nyp.edu.sg" },
                        { "Password", Utilities.GenerateUniqueID() },
                        { "ContactNumber", Utilities.GenerateRandomInt(10000000, 99999999).ToString() },
                        { "UserRole", "student" },
                        { "Avatar", "" },
                        { "EmailVerified", false },
                        { "PhoneVerified", false },
                        { "CurrentPoints", Utilities.GenerateRandomInt(100, 300) },
                        { "TotalPoints", Utilities.GenerateRandomInt(300, 600) }
                    }
                });

                var questList = _context.Quests.ToList();
                var studentsList = _context.Students.ToList();

                var classCount = classes.Count;
                var studentCount = studentsList.Count;
                var studentsPerClass = studentCount / classCount;
                var remainingStudents = studentCount % classCount;

                for (var i = 0; i < classCount; i++) {
                    for (var j = 0; j < studentsPerClass; j++) {
                        var student = studentsList[i * studentsPerClass + j];
                        var classStudent = new ClassStudents {
                            ClassID = classes[i].ClassID,
                            StudentID = student.StudentID
                        };

                        _context.ClassStudents.Add(classStudent);
                    }
                }

                for (var i = 0; i < remainingStudents; i++) {
                    var student = studentsList[studentCount - remainingStudents + i];
                    var classStudent = new ClassStudents {
                        ClassID = classes[classCount - 1].ClassID,
                        StudentID = student.StudentID
                    };

                    _context.ClassStudents.Add(classStudent);
                }

                await _context.SaveChangesAsync();

                for (int i = 0; i < 10; i++) {
                    var classPoints = new ClassPoints {
                        ClassID = _context.ClassStudents.ToList()[i].ClassID,
                        QuestID = questList[i].QuestID,
                        ContributingStudentID = studentsList[i].StudentID,
                        DateCompleted = DateTime.Now.AddDays(i - 7).ToString("yyyy-MM-dd"),
                        PointsAwarded = Utilities.GenerateRandomInt(10, 100)
                    };

                    _context.ClassPoints.Add(classPoints);
                }

                await _context.SaveChangesAsync();

                return;
            } catch (Exception ex) {
                Console.WriteLine("");
                Console.WriteLine($"ERROR: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");

                return;
            }
        }
    }

    public class Program {
        public static async Task Main(string[] args) {
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<MyDbContext>();

            if (args.Length > 0 && args[0].Equals("superuser", StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine("");
                Console.WriteLine("SERVER MODE: SUPERUSER");
            } else if (args.Length == 0) {
                Console.WriteLine("");
                Console.WriteLine("SERVER MODE: STANDARD");
            }

            using (var scope = builder.Services.BuildServiceProvider().CreateScope()) {
                var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                Bootcheck.Run(dbContext);
            }

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<Captcha>();
            builder.Services.AddScoped<CheckSystemLockedFilter>();
            builder.Services.AddScoped<MSAuth>();
            builder.Services.AddScoped<OpenAIChatService>();
            builder.Services.AddSingleton<IVectorStoreService, VectorStoreService>();
            builder.Services.AddTransient<RagOpenAIChatService>();

            builder.Services.AddCors(options => {
                options.AddPolicy("AllowSpecificOrigins", policy => {
                    var frontendUrl = "http://localhost:5173";
                    if (!string.IsNullOrEmpty(frontendUrl)) {
                        policy.WithOrigins(frontendUrl)
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
                if (string.IsNullOrEmpty(jwtKey)) {
                    throw new Exception("JWT secret key is missing.");
                }

                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddSwaggerGen(options => {
                var securityScheme = new OpenApiSecurityScheme {
                    In = ParameterLocation.Header,
                    Description = "Enter JWT Bearer token **_only_**",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                };

                options.AddSecurityDefinition("Bearer", securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    { securityScheme, new List<string>() }
                });
            });

            builder.WebHost.UseUrls("http://0.0.0.0:5082");

            builder.WebHost.ConfigureKestrel((context, options) => {
                var kestrelConfig = context.Configuration.GetSection("Kestrel:Endpoints");

                var httpUrl = kestrelConfig.GetValue<string>("Http:Url");
                if (!string.IsNullOrEmpty(httpUrl)) {
                    var httpPort = new Uri(httpUrl).Port;
                    options.Listen(IPAddress.Any, httpPort);
                }

                var httpsUrl = kestrelConfig.GetValue<string>("Https:Url");
                if (!string.IsNullOrEmpty(httpsUrl)) {
                    var httpsPort = new Uri(httpsUrl).Port;
                    options.Listen(IPAddress.Any, httpsPort, listenOptions => {
                        listenOptions.UseHttps();
                    });
                }
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope()) {
                var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

                if (Environment.GetEnvironmentVariable("DB_MODE") == "cloud") {
                    try {
                        if (await dbContext.Database.CanConnectAsync()) {
                            Console.WriteLine("Successfully connected to CloudSQL.");
                        } else {
                            Console.WriteLine("Failed to connect to CloudSQL.");
                        }
                    } catch (Exception ex) {
                        Console.WriteLine($"Error connecting to CloudSQL: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                    }
                } else if (Environment.GetEnvironmentVariable("DB_MODE") == "local") {
                    try {
                        dbContext.Database.EnsureCreated();
                        if (await dbContext.Database.CanConnectAsync()) {
                            Console.WriteLine("Successfully connected to local SQLite.");
                        } else {
                            Console.WriteLine("Failed to connect to local SQLite.");
                        }
                    } catch (Exception ex) {
                        Console.WriteLine($"Error connecting to local SQLite: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
                    }
                } else {
                    Console.WriteLine("Invalid DB_MODE configuration.");
                }
            }

            app.UseStaticFiles();

            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowSpecificOrigins");
            app.UseHttpsRedirection();
            app.MapControllers();

            app.UseAuthentication();
            app.UseAuthorization();

            Console.WriteLine();
            Console.Write($"Server running on {Environment.GetEnvironmentVariable("HTTPS_URL")}/swagger/index.html");
            Console.WriteLine();

            if (args.Length > 0 && args[0].Equals("superuser", StringComparison.OrdinalIgnoreCase))  {
                var serverTask = app.RunAsync();

                using (var scope = app.Services.CreateScope())  {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var msAuth = scope.ServiceProvider.GetRequiredService<MSAuth>();
                    var script = new SuperuserScript(dbContext, config, msAuth);
                    await script.Run();
                }

                await serverTask;
            } else if (args.Length == 0) {
                app.Run();
            }
        }
    }
}