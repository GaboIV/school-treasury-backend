namespace Application.DTOs
{
    public class ExpenseDto : BaseDto
    {
        public required string Id { get; set; }
        public required string ExpenseTypeId { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public required DateTime Date { get; set; }
        public decimal PercentagePaid { get; set; }
        public required TotalPaidDto Advance { get; set; }
        public required List<ImageDto> Images { get; set; }
    }
}