using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace IskoWalkAPI.Models
{
    public class WalkRequest
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        // Alias properties for backward compatibility
        public int RequesterId => UserId;
        public string RequesterName => User?.FullName ?? string.Empty;
        [Required]
        [MaxLength(200)]
        public string FromLocation { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? SpecifyOrigin { get; set; }
        [Required]
        [MaxLength(200)]
        public string ToDestination { get; set; } = string.Empty;
        // Alias for ToLocation
        public string ToLocation => ToDestination;
        [Required]
        public DateTime DateOfWalk { get; set; }
        // Alias for WalkDate
        public DateTime WalkDate => DateOfWalk;
        [Required]
        public TimeSpan TimeOfWalk { get; set; }
        // Alias for WalkTime
        public TimeSpan WalkTime => TimeOfWalk;
        [MaxLength(500)]
        public string? AdditionalNotes { get; set; }
        [MaxLength(500)]
        public string? AttireDescription { get; set; }
        [MaxLength(20)]
        public string? ContactNumber { get; set; }
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active";
        public int? CompanionId { get; set; }
        [ForeignKey("CompanionId")]
        public User? Companion { get; set; }
        public string? CompanionName => Companion?.FullName;
        
        // ✅ NEW: Accepted fields
        public int? AcceptedBy { get; set; }
        public DateTime? AcceptedAt { get; set; }
        
        [MaxLength(200)]
        public string? CancellationReason { get; set; }
        [MaxLength(500)]
        public string? CancellationDetails { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
