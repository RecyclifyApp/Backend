using System.Net;
using System.Text;
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

        public void Run() {
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
                    Console.WriteLine("7. Clean and populate database");
                    Console.WriteLine("8. Exit superuser script");

                    Console.WriteLine();
                    Console.Write("Enter action: ");

                    if (!int.TryParse(Console.ReadLine(), out int action)) {
                        Console.WriteLine("");
                        Console.WriteLine("ERROR: Please enter a valid integer.");
                        continue;
                    }

                    switch (action) {
                        case 1:
                            CreateAccount();
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
                            CleanAndPopulateDatabase();
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

        private void CreateAccount() {
            Console.WriteLine("");
            Console.WriteLine("Creating account...");
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

        private void CleanAndPopulateDatabase() {
            Console.WriteLine("");
            Console.WriteLine("Cleaning and populating database...");
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

            if (args.Length > 0 && args[0].Equals("superuser", StringComparison.OrdinalIgnoreCase)) 
            {
                // Start the server in the background
                var serverTask = app.RunAsync();

                // Optional: Add a delay to ensure server starts (adjust as needed)
                await Task.Delay(500);

                // Execute the superuser script while the server is running
                using (var scope = app.Services.CreateScope()) 
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var script = new SuperuserScript(dbContext, config);
                    script.Run();
                }

                // Keep the application running until the server task ends
                await serverTask;
            }
            else if (args.Length == 0) 
            {
                app.Run();
            }
        }
    }
}
