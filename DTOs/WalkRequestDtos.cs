using System;

namespace IskoWalkAPI.DTOs
{
    public class WalkRequestResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public string RequesterEmail { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string? SpecifyOrigin { get; set; }
        public string ToDestination { get; set; } = string.Empty;
        public DateTime DateOfWalk { get; set; }
        public TimeSpan TimeOfWalk { get; set; }
        public string? AttireDescription { get; set; }
        public string? AdditionalNotes { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? CompanionId { get; set; }
        public string? CompanionName { get; set; }
        public string? CancellationReason { get; set; }
        public string? CancellationDetails { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}

public class CancelRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class AcceptRequestDto
{
    public int RequestId { get; set; }
    public string CompanionId { get; set; } = string.Empty;
    public string CompanionName { get; set; } = string.Empty;
}
