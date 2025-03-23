using System;

namespace Application.DTOs
{
    public class RegisterCashboxMovementDto
    {
        public decimal Amount { get; set; }
        public string Concept { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
    }
} 