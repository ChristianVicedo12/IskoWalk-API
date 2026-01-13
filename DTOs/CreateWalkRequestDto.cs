using System;
using System.ComponentModel.DataAnnotations;

namespace IskoWalkAPI.DTOs
{
    public class CreateWalkRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "From location is required")]
        [MaxLength(200)]
        public string FromLocation { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? SpecifyOrigin { get; set; }
        
        [Required(ErrorMessage = "Destination is required")]
        [MaxLength(200)]
        public string ToDestination { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Date of walk is required")]
        public DateTime DateOfWalk { get; set; }
        
        [Required(ErrorMessage = "Time of walk is required")]
        public TimeSpan TimeOfWalk { get; set; }
        
        [MaxLength(500)]
        public string? AdditionalNotes { get; set; }
        
        [MaxLength(500)]
        public string? AttireDescription { get; set; }
        
        [MaxLength(20)]
        public string? ContactNumber { get; set; }
    }
}
