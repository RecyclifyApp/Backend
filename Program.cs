using Backend;
using Backend.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddDbContext<MyDbContext>();

Bootcheck.Run();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient<Captcha>();
builder.Services.AddScoped<Captcha>();

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
        In = ParameterLocation.Header, Description = "Token", 
        Name = "Authorization", Type = SecuritySchemeType.Http, 
        BearerFormat = "JWT", 
        Scheme = "Bearer", 
        Reference = new OpenApiReference { 
            Type = ReferenceType.SecurityScheme, Id = "Bearer" 
        } 
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
            DatabaseManager.SaveToSqlite(dbContext);
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

app.UseAuthentication();
app.UseAuthorization();

Console.WriteLine("");
Console.WriteLine($"Server running on {Environment.GetEnvironmentVariable("HTTPS_URL")}" + "/swagger/index.html");
Console.WriteLine("");

app.Run();
