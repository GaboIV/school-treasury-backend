using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateExpenseTypeDto
    {
        [Required]
        public string? Id { get; set; }
        
        [Required]
        public string? Name { get; set; }
    }
} 