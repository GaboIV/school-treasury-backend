using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateExpenseDto
    {
        [Required]
        public string? Id { get; set; }

        public string? Name { get; set; }

        [Required]
        public string? ExpenseTypeId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public decimal IndividualAmount { get; set; }
        public TotalPaidDto? Advance { get; set; }

        [Required]
        public string? StudentQuantity { get; set; }

        public string? Status { get; set; }

        public List<CreateImageDto>? Images { get; set; }
    }
}