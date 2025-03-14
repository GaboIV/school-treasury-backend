using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateCollectionTypeDto
    {
        [Required]
        public string? Name { get; set; }
    }
}