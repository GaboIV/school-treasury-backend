namespace Application.DTOs
{
    public class StudentDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = "001-boy.svg";
    }
} 