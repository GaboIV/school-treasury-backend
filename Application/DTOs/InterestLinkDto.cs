namespace Application.DTOs
{
    public class InterestLinkDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; } = 0;
    }
} 