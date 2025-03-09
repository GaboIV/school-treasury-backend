using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Application.DTOs
{
    public class CreateExpenseDto
    {
        public string? Name { get; set; }

        [Required]
        public string? ExpenseTypeId { get; set; }

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
