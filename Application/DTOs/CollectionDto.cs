using Domain.Entities;

namespace Application.DTOs
{
    public class CollectionDto : BaseDto
    {
        public string? CollectionTypeId { get; set; }
        public CollectionTypeDto? CollectionType { get; set; }
        public string? Name { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal IndividualAmount { get; set; }

        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
        public DateTime Date { get; set; }
        public decimal PercentagePaid { get; set; }
        public bool AllowsExemptions { get; set; }
        public Advance Advance { get; set; } = new Advance();
        public string? StudentQuantity { get; set; }
        public List<Image>? Images { get; set; }
    }
}