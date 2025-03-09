namespace Application.DTOs
{
    public class TotalPaidDto
    {
        public int? Total { get; set; } = 0;
        public int? Completed { get; set; } = 0;
        public int? Pending { get; set; } = 0;
    }
}