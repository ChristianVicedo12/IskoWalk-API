using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using IskoWalkAPI.Data;

namespace IskoWalkAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _context.Users.FindAsync(userId);

                if (user == null || user.IsDeleted)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = "Account deleted successfully" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = $"Error deleting account: {ex.Message}" 
                });
            }
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Getting profile for user: {userIdClaim}");
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _context.Users
                    .Where(u => u.Id == userId && !u.IsDeleted)
                    .Select(u => new {
                        id = u.Id,
                        fullName = u.FullName,
                        username = u.Username,
                        email = u.Email,
                        contactNumber = u.ContactNumber,
                        createdAt = u.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation($"Profile loaded: {user.fullName}");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "Error fetching profile", error = ex.Message });
            }
        }
    }
}
