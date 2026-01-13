namespace IskoWalkAPI.Models
{
    public class CreateWalkRequest
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string? AttireDescription { get; set; }
        public string? AdditionalNotes { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
    }
}
