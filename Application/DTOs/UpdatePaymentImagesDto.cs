using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class UpdatePaymentImagesDto
    {
        public string? Comment { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public DateTime? PaymentDate { get; set; }
    }
} 