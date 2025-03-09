namespace Application.DTOs
{
    public class ExpenseDto : BaseDto
    {
        public required string ExpenseTypeId { get; set; }
        public ExpenseTypeDto? ExpenseType { get; set; }
        public string? Name { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal IndividualAmount { get; set; }
        public required DateTime Date { get; set; }
        public decimal PercentagePaid { get; set; }
        public required TotalPaidDto Advance { get; set; }
        public string? StudentQuantity { get; set; }
        public required List<ImageDto> Images { get; set; }
    }
}