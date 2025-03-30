using Domain.Entities;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs
{
    public class StudentPaymentDto
    {
        public string? Id { get; set; }
        public string CollectionId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string? StudentName { get; set; }
        public string? CollectionName { get; set; }
        public decimal AmountCollection { get; set; }
        public decimal AdjustedAmountCollection { get; set; }
        public decimal AmountPaid { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public string? Voucher { get; set; }
        public decimal Excedent { get; set; }
        public decimal Surplus { get; set; }
        public decimal Pending { get; set; }
        public string? Comment { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    public class CreateStudentPaymentDto
    {
        public string CollectionId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; } = 0;
        public List<string> Images { get; set; } = new List<string>();
        public string? Voucher { get; set; }
        public string? Comment { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    
    public class UpdateStudentPaymentDto
    {
        public decimal AmountPaid { get; set; }
        public List<string>? Images { get; set; }
        public string? Voucher { get; set; }
        public string? Comment { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    
    public class UpdatePaymentDetailsDto
    {
        public List<string>? Images { get; set; }
        public string? Voucher { get; set; }
        public string? Comment { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    
    public class ExoneratePaymentDto
    {
        public string? Comment { get; set; }
        public List<Microsoft.AspNetCore.Http.IFormFile> Images { get; set; } = new List<Microsoft.AspNetCore.Http.IFormFile>();
        public DateTime? PaymentDate { get; set; }
    }
} 