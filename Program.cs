using System.Net;
using System.Text;
using System.Threading.Tasks;
using Backend.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Backend {
    class SuperuserScript {
        private readonly MyDbContext _context;
        private readonly IConfiguration _config;

        public SuperuserScript(MyDbContext context, IConfiguration config) {
            _context = context;
            _config = config;
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
                    Console.WriteLine("------Welcome to the Recyclify System Superuser Console------");
                    Console.WriteLine("1. Create account");
                    Console.WriteLine("2. Delete account");
                    Console.WriteLine("3. Lock system");
                    Console.WriteLine("4. Enable services");
                    Console.WriteLine("5. Disable services");
                    Console.WriteLine("6. Clear Firebase Cloud Storage");
                    Console.WriteLine("7. Wipe database");
                    Console.WriteLine("8. Populate Database");
                    Console.WriteLine("9. Exit");

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
                            DeleteAccount();
                            break;
                        case 3:
                            LockSystem();
                            break;
                        case 4:
                            EnableServices();
                            break;
                        case 5:
                            DisableServices();
                            break;
                        case 6:
                            ClearFirebaseCloudStorage();
                            break;
                        case 7:
                            WipeDatabase();
                            break;
                        case 8:
                            PopulateDatabase();
                            break;
                        case 9:
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
                            Console.WriteLine("ERROR: Please enter a valid integer from 1-9.");
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
                        Console.WriteLine($"ERROR: {ex.Message}");
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

                    var teacherKvp = new List<Dictionary<string, object>> {
                        new Dictionary<string, object> {
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
                        Console.WriteLine("");
                        Console.WriteLine("Teacher Account created successfully.");
                        Console.WriteLine("-------------------------------------------");
                        Console.WriteLine("Teacher Username: " + teacherKvp[0]["Email"]);
                        Console.WriteLine("Teacher Password: " + teacherKvp[0]["Password"]);
                        Console.WriteLine("-------------------------------------------");
                    } catch (Exception ex) {
                        Console.WriteLine("");
                        Console.WriteLine($"ERROR: {ex.Message}");
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
                        Console.WriteLine($"ERROR: {ex.Message}");
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
                        Console.WriteLine("");
                        Console.WriteLine("Student Account created successfully.");
                        Console.WriteLine("-------------------------------------------");
                        Console.WriteLine("Student Username: " + studentKvp[0]["Email"]);
                        Console.WriteLine("Student Password: " + studentKvp[0]["Password"]);
                        Console.WriteLine("-------------------------------------------");
                    } catch (Exception ex) {
                        Console.WriteLine("");
                        Console.WriteLine($"ERROR: {ex.Message}");
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

        private void DeleteAccount() {
            Console.WriteLine("");
            Console.WriteLine("Deleting account...");
        }

        private void LockSystem() {
            Console.WriteLine("");
            Console.WriteLine("Locking system...");
        }

        private void EnableServices() {
            Console.WriteLine("");
            Console.WriteLine("Enabling services...");
        }

        private void DisableServices() {
            Console.WriteLine("");
            Console.WriteLine("Disabling services...");
        }

        private void ClearFirebaseCloudStorage() {
            Console.WriteLine("");
            Console.WriteLine("Clearing Firebase Cloud Storage...");
        }

        private async void WipeDatabase() {
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

            await _context.SaveChangesAsync();

            Console.WriteLine("");
            Console.WriteLine("SUCCESS: DATABASE WIPED.");
        }

        private void PopulateDatabase() {
            Console.WriteLine("");
            Console.WriteLine("Populating database...");
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
            } else {
                Console.WriteLine("");
                Console.WriteLine("Invalid command line argument.");
                Environment.Exit(0);
                return;
            }

            Bootcheck.Run();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();

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
                        Console.WriteLine($"Error connecting to CloudSQL: {ex.Message}");
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
                        Console.WriteLine($"Error connecting to local SQLite: {ex.Message}");
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
                    var script = new SuperuserScript(dbContext, config);
                    await script.Run();
                }

                await serverTask;
            } else if (args.Length == 0) {
                app.Run();
            }
        }
    }
}
