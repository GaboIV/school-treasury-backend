namespace Application.DTOs
{
    public class AdjustExpenseAmountDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal AdjustedAmount { get; set; }
        public decimal Surplus { get; set; }
    }
} 