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

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

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
}

app.UseStaticFiles();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

Console.WriteLine("");
Console.WriteLine($"Server running on {Environment.GetEnvironmentVariable("HTTPS_URL")}" + "/swagger/index.html");
Console.WriteLine("");

app.Run();
