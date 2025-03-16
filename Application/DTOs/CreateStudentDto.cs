using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateStudentDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; } = string.Empty;
        
        public string? Avatar { get; set; }
    }
} 