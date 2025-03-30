using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Application.DTOs
{
    public class CreateCollectionDto
    {
        public string? Name { get; set; }

        [Required]
        public string? CollectionTypeId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }
    
        [Required]
        public string? StudentQuantity { get; set; }

        public bool? Status { get; set; }

        public bool AllowsExemptions { get; set; } = false;

        public List<CreateImageDto>? Images { get; set; }
    }
}
