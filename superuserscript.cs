using Backend.Services;
using Backend.Models;
using Backend;
using DotNetEnv;

// Create a superuserscript for me that allows me to create accounts and delete accounts for all types: student, teacher, parent and admin. it should also allow me to lock the system, enable and disable services and trigger the cleanAndPopulateDatabase function.

// Please modify the cleanAndPopulateDatabase function to remove all hardcoded data entries for creating of user accounts, because all accounts will be created from this superuserscript, including admin accounts.

// For account creation, ask for all required fields, just like the CreateUserRecords function.

// To be able to run this script, please prompt for superuser username, password and PIN. store these 3 in the .env file. this is to ensure only ppl who have the credentials can execute this script.

// also allow the superuser to clear all files in firebase cloud storage. add a new function to do this if necessary.

class SuperuserScript {
    private readonly MyDbContext _context;
    private readonly IConfiguration _config;

    public SuperuserScript(MyDbContext context, IConfiguration config) {
        _context = context;
        _config = config;
    }

    private void CreateAccount() {
        Console.WriteLine("Creating account...");
    }

    private void DeleteAccount() {
        Console.WriteLine("Deleting account...");
    }

    private void LockSystem() {
        Console.WriteLine("Locking system...");
    }

    private void EnableServices() {
        Console.WriteLine("Enabling services...");
    }

    private void DisableServices() {
        Console.WriteLine("Disabling services...");
    }

    private void ClearFirebaseCloudStorage() {
        Console.WriteLine("Clearing Firebase Cloud Storage...");
    }

    private void CleanAndPopulateDatabase() {
        Console.WriteLine("Cleaning and populating database...");
    }

    public void Run() {
        // Prompt for superuser username, password and PIN
        Console.WriteLine("Enter superuser username: ");
        string superuserUsername = Console.ReadLine() ?? "";
        Console.WriteLine("Enter superuser password: ");
        string superuserPassword = Console.ReadLine() ?? "";
        Console.WriteLine("Enter superuser PIN: ");
        string superuserPIN = Console.ReadLine() ?? "";

        if (superuserUsername != _config["SUPERUSER_USERNAME"] || superuserPassword != _config["SUPERUSER_PASSWORD"] || superuserPIN != _config["SUPERUSER_PIN"]) {
            Console.WriteLine("ACCESS UNAUTHORISED: Invalid superuser credentials.");
            Environment.Exit(0);
            return;
        }

        // Prompt for action
        Console.WriteLine("");
        Console.WriteLine("1. Create account");
        Console.WriteLine("2. Delete account");
        Console.WriteLine("3. Lock system");
        Console.WriteLine("4. Enable services");
        Console.WriteLine("5. Disable services");
        Console.WriteLine("6. Clear Firebase Cloud Storage");
        Console.WriteLine("7. Clean and populate database");
        Console.WriteLine("8. Exit");
        Console.WriteLine("");

        Console.WriteLine("Enter action: ");
        Console.WriteLine("");

        int action = int.Parse(Console.ReadLine() ?? "");

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
                Console.WriteLine("Script terminated. Goodbye!");
                Environment.Exit(0);
                return;
            default:
                Console.WriteLine("ERROR: Invalid action.");
                break;
        }
    }
}