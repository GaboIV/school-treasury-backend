using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class PaymentRequestDto
    {
        public string? Id { get; set; }
        public string CollectionId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string? StudentName { get; set; }
        public string? CollectionName { get; set; }
        public decimal AmountCollection { get; set; }
        public decimal AmountPaid { get; set; }
        public PaymentRequestStatus Status { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public string? Voucher { get; set; }
        public string? Comment { get; set; }
        public DateTime? PaymentDate { get; set; }
        public List<PaymentRequestHistoryEntryDto> HistoryEntries { get; set; } = new List<PaymentRequestHistoryEntryDto>();
        public List<AdminCommentDto> AdminComments { get; set; } = new List<AdminCommentDto>();
        public string? RejectionReason { get; set; }
        public string? ApprovedByAdminId { get; set; }
        public string? ApprovedByAdminName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? StudentPaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    public class PaymentRequestHistoryEntryDto
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public PaymentRequestStatus? PreviousStatus { get; set; }
        public PaymentRequestStatus? NewStatus { get; set; }
    }
    
    public class AdminCommentDto
    {
        public DateTime Timestamp { get; set; }
        public string AdminId { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
    }
    
    public class CreatePaymentRequestDto
    {
        public string CollectionId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string? Comment { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    
    public class CreatePaymentRequestWithImagesDto
    {
        public string CollectionId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string? Comment { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public DateTime? PaymentDate { get; set; }
    }
    
    public class UpdatePaymentRequestDto
    {
        public decimal AmountPaid { get; set; }
        public List<string>? Images { get; set; }
        public string? Voucher { get; set; }
        public string? Comment { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    
    public class UpdatePaymentRequestWithImagesDto
    {
        public decimal AmountPaid { get; set; }
        public string? Comment { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public DateTime? PaymentDate { get; set; }
    }
    
    public class ApprovePaymentRequestDto
    {
        public string AdminId { get; set; } = string.Empty;
        public string? AdminComment { get; set; }
    }
    
    public class RejectPaymentRequestDto
    {
        public string AdminId { get; set; } = string.Empty;
        public string RejectionReason { get; set; } = string.Empty;
    }
    
    public class RequestChangesDto
    {
        public string AdminId { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
    }
    
    public class AddAdminCommentDto
    {
        public string AdminId { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
    }
} 