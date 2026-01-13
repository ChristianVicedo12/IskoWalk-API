using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IskoWalkAPI.Models;
using IskoWalkAPI.Data;
using BCrypt.Net;

namespace IskoWalkAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] UserSignUpRequest request)
    {
        try
        {
            // DEBUG: Log what we received
            _logger.LogInformation($"=== SIGNUP DEBUG ===");
            _logger.LogInformation($"FullName received: '{request.FullName}'");
            _logger.LogInformation($"Email received: '{request.Email}'");
            _logger.LogInformation($"ContactNumber received: '{request.ContactNumber}'");
            _logger.LogInformation($"FullName is null or empty: {string.IsNullOrEmpty(request.FullName)}");
            
            var existingUser = await _context.Users
                .Where(u => u.Email == request.Email && !u.IsDeleted)
                .FirstOrDefaultAsync();
                
            if (existingUser != null)
            {
                return BadRequest(new { success = false, message = "Email already registered" });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = passwordHash,
                ContactNumber = request.ContactNumber,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _logger.LogInformation($"About to save user with FullName: '{user.FullName}'");
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"User saved! Id: {user.Id}, FullName from DB: '{user.FullName}'");

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                message = "Sign up successful",
                token = token,
                user = new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    contactNumber = user.ContactNumber
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] UserSignInRequest request)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Email == request.Email && !u.IsDeleted)
                .FirstOrDefaultAsync();
            
            if (user == null)
            {
                return BadRequest(new { success = false, message = "Invalid credentials" });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            
            if (!isPasswordValid)
            {
                return BadRequest(new { success = false, message = "Invalid credentials" });
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                message = "Login successful",
                token = token,
                user = new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    contactNumber = user.ContactNumber
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signin");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { success = true, message = "Logged out successfully" });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class UserSignUpRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
}

public class UserSignInRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
