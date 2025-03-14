using Domain.Entities;
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class CollectionDto : BaseDto
    {
        public string? CollectionTypeId { get; set; }
        public CollectionTypeDto? CollectionType { get; set; }
        public string? Name { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal IndividualAmount { get; set; }
        public decimal? AdjustedIndividualAmount { get; set; }
        public decimal TotalSurplus { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
        public DateTime Date { get; set; }
        public decimal PercentagePaid { get; set; }
        public Advance Advance { get; set; } = new Advance();
        public string? StudentQuantity { get; set; }
        public List<Image>? Images { get; set; }
    }
}