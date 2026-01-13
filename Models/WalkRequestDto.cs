using System;

namespace IskoWalkAPI.Models
{
    public class WalkRequestDto
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public TimeSpan WalkTime { get; set; }  // Changed to TimeSpan
        public DateTime WalkDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CompanionName { get; set; }
        public string? AdditionalNotes { get; set; }
        public DateTime CreatedAt { get; set; }

        public static WalkRequestDto FromWalkRequest(WalkRequest request)
        {
            return new WalkRequestDto
            {
                Id = request.Id,
                RequesterId = request.RequesterId,
                RequesterName = request.RequesterName,
                FromLocation = request.FromLocation,
                ToLocation = request.ToLocation,
                WalkTime = request.WalkTime,  // Direct assignment
                WalkDate = request.WalkDate,
                Status = request.Status,
                CompanionName = request.CompanionName,
                AdditionalNotes = request.AdditionalNotes,
                CreatedAt = request.CreatedAt
            };
        }
    }
}
