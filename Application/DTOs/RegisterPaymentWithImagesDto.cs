using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class RegisterPaymentWithImagesDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string? Comment { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public DateTime? PaymentDate { get; set; }
    }
} 