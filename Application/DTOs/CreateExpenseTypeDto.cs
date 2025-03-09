using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateExpenseTypeDto
    {
        [Required]
        public string? Name { get; set; }
    }
}