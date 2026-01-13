using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IskoWalkAPI.Data;
using IskoWalkAPI.Models;
using IskoWalkAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IskoWalkAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WalkRequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WalkRequestController> _logger;

        public WalkRequestController(ApplicationDbContext context, ILogger<WalkRequestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<WalkRequestResponseDto>>> GetAvailableRequests()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"User ID from token: {userIdClaim}");

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var requests = await _context.WalkRequests
                    .Include(w => w.User)
                    .Where(w => w.UserId != userId && w.Status == "Active")
                    .OrderByDescending(w => w.CreatedAt)
                    .Select(w => new WalkRequestResponseDto
                    {
                        Id = w.Id,
                        UserId = w.UserId,
                        RequesterName = w.User!.FullName,
                        RequesterEmail = w.User.Email,
                        ContactNumber = w.User.ContactNumber,
                        FromLocation = w.FromLocation,
                        SpecifyOrigin = w.SpecifyOrigin,
                        ToDestination = w.ToDestination,
                        DateOfWalk = w.DateOfWalk,
                        TimeOfWalk = w.TimeOfWalk,
                        AttireDescription = w.AttireDescription,
                        AdditionalNotes = w.AdditionalNotes,
                        Status = w.Status,
                        CreatedAt = w.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {requests.Count} available requests");
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available requests");
                return StatusCode(500, new { message = "Error fetching requests", error = ex.Message });
            }
        }

        [HttpPost("{requestId}/accept")]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Accept request - User ID: {userIdClaim}, Request ID: {requestId}");

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var request = await _context.WalkRequests
                    .FirstOrDefaultAsync(w => w.Id == requestId);

                if (request == null)
                {
                    return NotFound(new { message = "Request not found" });
                }

                if (request.UserId == userId)
                {
                    return BadRequest(new { message = "You cannot accept your own request" });
                }

                if (request.Status != "Active")
                {
                    return BadRequest(new { message = "Request is not available" });
                }

                request.Status = "Accepted";
                request.AcceptedBy = userId;
                request.AcceptedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Request {requestId} accepted by user {userId}");
                return Ok(new { success = true, message = "Request accepted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting request");
                return StatusCode(500, new { message = "Error accepting request", error = ex.Message });
            }
        }

        [HttpGet("my-accepted")]
        public async Task<ActionResult<List<object>>> GetMyAcceptedRequests()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Getting accepted requests for user: {userIdClaim}");

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Load all data first, then project
                var requests = await _context.WalkRequests
                    .Include(w => w.User)
                    .Where(w => w.AcceptedBy == userId && w.Status == "Accepted")
                    .OrderByDescending(w => w.AcceptedAt)
                    .ToListAsync();

                _logger.LogInformation($"Found {requests.Count} accepted requests");

                // Project after loading to avoid query translation issues
                var result = requests.Select(w => new
                {
                    id = w.Id,
                    userId = w.UserId,
                    requesterName = w.User?.FullName ?? "Unknown User",
                    requesterEmail = w.User?.Email ?? "",
                    contactNumber = w.User?.ContactNumber,
                    fromLocation = w.FromLocation,
                    specifyOrigin = w.SpecifyOrigin,
                    toDestination = w.ToDestination,
                    dateOfWalk = w.DateOfWalk,
                    timeOfWalk = w.TimeOfWalk,
                    attireDescription = w.AttireDescription,
                    additionalNotes = w.AdditionalNotes,
                    status = w.Status,
                    createdAt = w.CreatedAt,
                    acceptedAt = w.AcceptedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accepted requests");
                return StatusCode(500, new { message = "Error fetching accepted requests", error = ex.Message });
            }
        }

        [HttpPost("{requestId}/cancel")]
        public async Task<IActionResult> CancelRequest(int requestId, [FromBody] CancelRequestDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Cancel request - User ID: {userIdClaim}, Request ID: {requestId}");

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var request = await _context.WalkRequests.FirstOrDefaultAsync(w => w.Id == requestId);

                if (request == null)
                {
                    return NotFound(new { message = "Request not found" });
                }

                if (request.UserId != userId && request.AcceptedBy != userId)
                {
                    return Forbid();
                }

                if (request.Status == "Completed" || request.Status == "Cancelled")
                {
                    return BadRequest(new { message = "Request already finalized" });
                }

                request.Status = "Cancelled";
                request.CancellationReason = dto.Reason ?? "No reason provided";
                request.CancelledAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Request {requestId} cancelled by user {userId}");
                return Ok(new { success = true, message = "Request cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling request");
                return StatusCode(500, new { message = "Error cancelling request", error = ex.Message });
            }
        }

        [HttpPost("{requestId}/complete")]
        public async Task<IActionResult> CompleteRequest(int requestId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Complete request - User ID: {userIdClaim}, Request ID: {requestId}");

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var request = await _context.WalkRequests.FirstOrDefaultAsync(w => w.Id == requestId);

                if (request == null)
                {
                    return NotFound(new { message = "Request not found" });
                }

                if (request.UserId != userId && request.AcceptedBy != userId)
                {
                    return Forbid();
                }

                if (request.Status != "Accepted")
                {
                    return BadRequest(new { message = "Request must be accepted first" });
                }

                request.Status = "Completed";
                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Request {requestId} completed by user {userId}");
                return Ok(new { success = true, message = "Request completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing request");
                return StatusCode(500, new { message = "Error completing request", error = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateWalkRequestDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Creating walk request for user: {userIdClaim}");

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var walkRequest = new WalkRequest
                {
                    UserId = userId,
                    FromLocation = dto.FromLocation,
                    SpecifyOrigin = dto.SpecifyOrigin,
                    ToDestination = dto.ToDestination,
                    DateOfWalk = dto.DateOfWalk,
                    TimeOfWalk = dto.TimeOfWalk,
                    AttireDescription = dto.AttireDescription,
                    AdditionalNotes = dto.AdditionalNotes,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.WalkRequests.Add(walkRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Walk request created successfully with ID: {walkRequest.Id}");

                return Ok(new
                {
                    success = true,
                    message = "Walk request created successfully",
                    requestId = walkRequest.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating walk request: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error creating request", error = ex.Message });
            }
        }

        [HttpGet("my-active")]
        public async Task<IActionResult> GetMyActiveRequests()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var requests = await _context.WalkRequests
                    .Include(w => w.User)
                    .Where(w => w.UserId == userId && (w.Status == "Active" || w.Status == "Accepted"))
                    .OrderByDescending(w => w.CreatedAt)
                    .ToListAsync();

                var result = requests.Select(w => new
                {
                    id = w.Id,
                    userId = w.UserId,
                    requesterName = w.User?.FullName ?? "You",
                    requesterEmail = w.User?.Email ?? "",
                    companionId = w.AcceptedBy,
                    companionName = w.AcceptedBy.HasValue ? _context.Users.FirstOrDefault(u => u.Id == w.AcceptedBy.Value)?.FullName : null,
                    fromLocation = w.FromLocation,
                    specifyOrigin = w.SpecifyOrigin,
                    toDestination = w.ToDestination,
                    dateOfWalk = w.DateOfWalk,
                    timeOfWalk = w.TimeOfWalk,
                    attireDescription = w.AttireDescription,
                    additionalNotes = w.AdditionalNotes,
                    status = w.Status,
                    createdAt = w.CreatedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching requests: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching requests", error = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<object>>> GetHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Getting history for user: {userIdClaim}");

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var history = await _context.WalkRequests
                    .Include(w => w.User)
                    .Where(w => (w.UserId == userId || w.AcceptedBy == userId) 
                             && (w.Status == "Completed" || w.Status == "Cancelled"))
                    .OrderByDescending(w => w.UpdatedAt ?? w.CreatedAt)
                    .ToListAsync();

                var result = history.Select(w => new
                {
                    id = w.Id,
                    userId = w.UserId,
                    requesterName = w.User?.FullName ?? "Unknown User",
                    companionId = w.AcceptedBy,
                    companionName = w.AcceptedBy.HasValue ? _context.Users.FirstOrDefault(u => u.Id == w.AcceptedBy.Value)?.FullName : null,
                    companionContact = w.AcceptedBy.HasValue ? _context.Users.FirstOrDefault(u => u.Id == w.AcceptedBy.Value)?.ContactNumber : null,
                    fromLocation = w.FromLocation,
                    specifyOrigin = w.SpecifyOrigin,
                    toDestination = w.ToDestination,
                    dateOfWalk = w.DateOfWalk,
                    timeOfWalk = w.TimeOfWalk,
                    attireDescription = w.AttireDescription,
                    additionalNotes = w.AdditionalNotes,
                    status = w.Status,
                    createdAt = w.CreatedAt,
                    completedAt = w.UpdatedAt
                }).ToList();

                _logger.LogInformation($"Found {result.Count} history items");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history");
                return StatusCode(500, new { message = "Error fetching history", error = ex.Message });
            }
        }
    }
}
