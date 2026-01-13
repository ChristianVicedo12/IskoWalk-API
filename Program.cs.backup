using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IskoWalkAPI.Data;
using IskoWalkAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure MySQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ✅ Register MySqlService
builder.Services.AddScoped<MySqlService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// Add validation for secret key
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException(
        "JWT SecretKey is not configured. Please add 'JwtSettings:SecretKey' to appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5180", "http://localhost:5181", "https://localhost:5180")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ✅ CRITICAL: Test Database Connection IMMEDIATELY
Console.WriteLine("=================================================");
Console.WriteLine("🔍 Testing MySQL Database Connection...");
Console.WriteLine("=================================================");

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        Console.WriteLine($"📊 Connection String: {connectionString}");
        
        var canConnect = db.Database.CanConnect();
        
        if (canConnect)
        {
            Console.WriteLine("✅ MySQL Connected Successfully!");
        }
        else
        {
            Console.WriteLine("❌ MySQL Connection Failed - CanConnect returned false");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("❌ MySQL Connection Error:");
    Console.WriteLine($"   Message: {ex.Message}");
    Console.WriteLine($"   Type: {ex.GetType().Name}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
    }
}

Console.WriteLine("=================================================\n");

// ✅ ADD TEST ENDPOINT
app.MapGet("/test-connection", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (canConnect)
        {
            return Results.Ok(new { 
                status = "success", 
                message = "✅ Connected to MySQL!",
                timestamp = DateTime.Now
            });
        }
        return Results.Problem("Cannot connect to database");
    }
    catch (Exception ex)
    {
        return Results.Problem($"❌ Error: {ex.Message}");
    }
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();