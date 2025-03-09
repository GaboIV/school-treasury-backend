namespace Application.DTOs
{
    public class ImageDto : BaseDto
    {
        public required string Id { get; set; }
        public required string Url { get; set; }
    }
}