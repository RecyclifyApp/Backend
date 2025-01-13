using Backend;
using Backend.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddDbContext<MyDbContext>();

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

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

    if (Environment.GetEnvironmentVariable("DB_MODE") == "cloud") {
        try {
            var connectedSuccesssfully = await dbContext.Database.CanConnectAsync();
            if (connectedSuccesssfully) {
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
            var connectedSuccesssfully = await dbContext.Database.CanConnectAsync();
            Utilities.SaveToSqlite(dbContext);
            if (connectedSuccesssfully) {
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

Console.WriteLine("");
Console.WriteLine($"Server running on {Environment.GetEnvironmentVariable("HTTPS_URL")}" + "/swagger/index.html");
Console.WriteLine("");

app.Run();
