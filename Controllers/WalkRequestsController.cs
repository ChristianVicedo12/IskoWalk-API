using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IskoWalkAPI.Data;
using IskoWalkAPI.DTOs;
using IskoWalkAPI.Models;
using System.Security.Claims;

namespace IskoWalkAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WalkRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public WalkRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
        
        // GET: api/walkrequests/available
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRequests()
        {
            var currentUserId = GetCurrentUserId();
            
            var requests = await _context.WalkRequests
                .Include(w => w.User)
                .Where(w => w.Status == "Active" && w.UserId != currentUserId)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WalkRequestResponseDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    RequesterName = w.User != null ? w.User.FullName : "",
                    RequesterEmail = w.User != null ? w.User.Email : "",
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
            
            return Ok(requests);
        }
        
        // GET: api/walkrequests/my-requests
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var currentUserId = GetCurrentUserId();
            
            var requests = await _context.WalkRequests
                .Include(w => w.User)
                .Include(w => w.Companion)
                .Where(w => w.UserId == currentUserId)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WalkRequestResponseDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    RequesterName = w.User != null ? w.User.FullName : "",
                    RequesterEmail = w.User != null ? w.User.Email : "",
                    CompanionId = w.CompanionId,
                    CompanionName = w.Companion != null ? w.Companion.FullName : null,
                    FromLocation = w.FromLocation,
                    SpecifyOrigin = w.SpecifyOrigin,
                    ToDestination = w.ToDestination,
                    DateOfWalk = w.DateOfWalk,
                    TimeOfWalk = w.TimeOfWalk,
                    AttireDescription = w.AttireDescription,
                    AdditionalNotes = w.AdditionalNotes,
                    Status = w.Status,
                    CreatedAt = w.CreatedAt,
                    AcceptedAt = w.AcceptedAt,
                    CancellationReason = w.CancellationReason
                })
                .ToListAsync();
            
            return Ok(requests);
        }
        
        // GET: api/walkrequests/accepted-walks
        [HttpGet("accepted-walks")]
        public async Task<IActionResult> GetAcceptedWalks()
        {
            var currentUserId = GetCurrentUserId();
            
            Console.WriteLine($"[DEBUG] GetAcceptedWalks called by user: {currentUserId}");
            
            var allRequests = await _context.WalkRequests
                .Include(w => w.User)
                .Include(w => w.Companion)
                .Where(w => w.AcceptedBy == currentUserId || w.CompanionId == currentUserId)
                .ToListAsync();
            
            Console.WriteLine($"[DEBUG] Total requests for user: {allRequests.Count}");
            foreach (var r in allRequests)
            {
                Console.WriteLine($"[DEBUG] Request ID: {r.Id}, Status: '{r.Status}'");
            }
            
            var acceptedRequests = allRequests
                .Where(w => w.Status == "Accepted")
                .OrderByDescending(w => w.AcceptedAt)
                .Select(w => new WalkRequestResponseDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    RequesterName = w.User != null ? w.User.FullName : "",
                    RequesterEmail = w.User != null ? w.User.Email : "",
                    CompanionId = w.AcceptedBy ?? w.CompanionId,
                    CompanionName = w.Companion != null ? w.Companion.FullName : "",
                    FromLocation = w.FromLocation,
                    SpecifyOrigin = w.SpecifyOrigin,
                    ToDestination = w.ToDestination,
                    DateOfWalk = w.DateOfWalk,
                    TimeOfWalk = w.TimeOfWalk,
                    AttireDescription = w.AttireDescription,
                    AdditionalNotes = w.AdditionalNotes,
                    Status = w.Status,
                    CreatedAt = w.CreatedAt,
                    AcceptedAt = w.AcceptedAt
                })
                .ToList();
            
            Console.WriteLine($"[DEBUG] Filtered accepted requests: {acceptedRequests.Count}");
            
            return Ok(acceptedRequests);
        }
        
        // GET: api/walkrequests/history
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var currentUserId = GetCurrentUserId();
            
            var requests = await _context.WalkRequests
                .Include(w => w.User)
                .Include(w => w.Companion)
                .Where(w => (w.UserId == currentUserId || w.CompanionId == currentUserId || w.AcceptedBy == currentUserId) 
                    && (w.Status == "Completed" || w.Status == "Cancelled"))
                .OrderByDescending(w => w.UpdatedAt ?? w.CancelledAt)
                .Select(w => new WalkRequestResponseDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    RequesterName = w.User != null ? w.User.FullName : "",
                    RequesterEmail = w.User != null ? w.User.Email : "",
                    CompanionId = w.CompanionId,
                    CompanionName = w.Companion != null ? w.Companion.FullName : null,
                    FromLocation = w.FromLocation,
                    SpecifyOrigin = w.SpecifyOrigin,
                    ToDestination = w.ToDestination,
                    DateOfWalk = w.DateOfWalk,
                    TimeOfWalk = w.TimeOfWalk,
                    AttireDescription = w.AttireDescription,
                    AdditionalNotes = w.AdditionalNotes,
                    Status = w.Status,
                    CreatedAt = w.CreatedAt,
                    AcceptedAt = w.AcceptedAt,
                    CancellationReason = w.CancellationReason
                })
                .ToListAsync();
            
            return Ok(requests);
        }
        
        // GET: api/walkrequests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequest(int id)
        {
            var request = await _context.WalkRequests
                .Include(w => w.User)
                .Include(w => w.Companion)
                .Where(w => w.Id == id)
                .Select(w => new WalkRequestResponseDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    RequesterName = w.User != null ? w.User.FullName : "",
                    RequesterEmail = w.User != null ? w.User.Email : "",
                    CompanionId = w.CompanionId,
                    CompanionName = w.Companion != null ? w.Companion.FullName : null,
                    FromLocation = w.FromLocation,
                    SpecifyOrigin = w.SpecifyOrigin,
                    ToDestination = w.ToDestination,
                    DateOfWalk = w.DateOfWalk,
                    TimeOfWalk = w.TimeOfWalk,
                    AttireDescription = w.AttireDescription,
                    AdditionalNotes = w.AdditionalNotes,
                    Status = w.Status,
                    CreatedAt = w.CreatedAt,
                    AcceptedAt = w.AcceptedAt,
                    CancellationReason = w.CancellationReason
                })
                .FirstOrDefaultAsync();
            
            if (request == null)
            {
                return NotFound(new { message = "Walk request not found" });
            }
            
            return Ok(request);
        }
        
        // POST: api/walkrequests
        [HttpPost]
        public async Task<ActionResult<WalkRequest>> CreateRequest(WalkRequest request)
        {
            var currentUserId = GetCurrentUserId();
            
            request.UserId = currentUserId;
            request.CreatedAt = DateTime.UtcNow;
            request.Status = "Active";
            
            _context.WalkRequests.Add(request);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
        }
        
        // POST: api/walkrequests/{id}/accept
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            var currentUserId = GetCurrentUserId();
            
            var request = await _context.WalkRequests.FindAsync(id);
            
            if (request == null)
            {
                return NotFound(new { message = "Walk request not found" });
            }
            
            if (request.Status != "Active")
            {
                return BadRequest(new { message = "This request has already been accepted or cancelled" });
            }
            
            if (request.UserId == currentUserId)
            {
                return BadRequest(new { message = "You cannot accept your own request" });
            }
            
            request.AcceptedBy = currentUserId;
            request.CompanionId = currentUserId;
            request.Status = "Accepted";
            request.AcceptedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Request accepted successfully" });
        }
        
        // POST: api/walkrequests/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelRequest(int id, [FromBody] CancelRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();
            
            var request = await _context.WalkRequests.FindAsync(id);
            
            if (request == null)
            {
                return NotFound(new { message = "Walk request not found" });
            }
            
            if (request.UserId != currentUserId)
            {
                return Forbid();
            }
            
            if (request.Status == "Cancelled" || request.Status == "Completed")
            {
                return BadRequest(new { message = "Cannot cancel this request" });
            }
            
            request.Status = "Cancelled";
            request.CancellationReason = dto.Reason;
            request.CancelledAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Request cancelled successfully" });
        }
        
        // POST: api/walkrequests/{id}/complete
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteRequest(int id)
        {
            var currentUserId = GetCurrentUserId();
            
            Console.WriteLine($"[DEBUG] CompleteRequest called by user {currentUserId} for request {id}");
            
            var request = await _context.WalkRequests.FindAsync(id);
            
            if (request == null)
            {
                Console.WriteLine($"[DEBUG] Request {id} not found");
                return NotFound(new { message = "Walk request not found" });
            }
            
            Console.WriteLine($"[DEBUG] Request {id} current status: '{request.Status}'");
            
            bool isCompanion = (request.AcceptedBy.HasValue && request.AcceptedBy.Value == currentUserId) || 
                              (request.CompanionId.HasValue && request.CompanionId.Value == currentUserId);
            bool isRequester = request.UserId == currentUserId;
            
            Console.WriteLine($"[DEBUG] isCompanion: {isCompanion}, isRequester: {isRequester}");
            
            if (!isCompanion && !isRequester)
            {
                Console.WriteLine($"[DEBUG] User {currentUserId} not authorized");
                return Forbid();
            }
            
            if (request.Status != "Accepted")
            {
                Console.WriteLine($"[DEBUG] Cannot complete - status is '{request.Status}', not 'Accepted'");
                return BadRequest(new { message = $"Only accepted requests can be marked as completed. Current status: {request.Status}" });
            }
            
            request.Status = "Completed";
            request.UpdatedAt = DateTime.UtcNow;
            
            Console.WriteLine($"[DEBUG] Saving changes - new status: '{request.Status}'");
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"[DEBUG] Request {id} marked as completed successfully");
            
            return Ok(new { message = "Request completed successfully" });
        }
    }
}
