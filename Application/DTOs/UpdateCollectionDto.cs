using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateCollectionDto
    {
        [Required]
        public string? Id { get; set; }

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

        public List<CreateImageDto>? Images { get; set; }
    }
}