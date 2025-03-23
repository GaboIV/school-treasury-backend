using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class PaymentRequest : BaseEntity
    {
        [BsonElement("collectionId")]
        public string CollectionId { get; set; } = string.Empty;
        
        [BsonElement("studentId")]
        public string StudentId { get; set; } = string.Empty;
        
        [BsonElement("amountPaid")]
        public decimal AmountPaid { get; set; } = 0;
        
        [BsonElement("pendingAmount")]
        public decimal PendingAmount { get; set; } = 0;
        
        [BsonElement("requestStatus")]
        public PaymentRequestStatus Status { get; set; } = PaymentRequestStatus.Pending;
        
        [BsonElement("images")]
        public List<string> Images { get; set; } = new List<string>();
        
        [BsonElement("voucher")]
        public string? Voucher { get; set; }
        
        [BsonElement("comment")]
        public string? Comment { get; set; }
        
        [BsonElement("paymentDate")]
        public DateTime? PaymentDate { get; set; }
        
        [BsonElement("historyEntries")]
        public List<PaymentRequestHistoryEntry> HistoryEntries { get; set; } = new List<PaymentRequestHistoryEntry>();
        
        [BsonElement("adminComments")]
        public List<AdminComment> AdminComments { get; set; } = new List<AdminComment>();
        
        [BsonElement("rejectionReason")]
        public string? RejectionReason { get; set; }
        
        [BsonElement("approvedByAdminId")]
        public string? ApprovedByAdminId { get; set; }
        
        [BsonElement("approvedAt")]
        public DateTime? ApprovedAt { get; set; }
        
        [BsonElement("studentPaymentId")]
        public string? StudentPaymentId { get; set; }
    }
    
    public enum PaymentRequestStatus
    {
        Pending,
        UnderReview,
        Approved,
        Rejected,
        NeedsChanges
    }
    
    public class PaymentRequestHistoryEntry
    {
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [BsonElement("action")]
        public string Action { get; set; } = string.Empty;
        
        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;
        
        [BsonElement("userRole")]
        public string UserRole { get; set; } = string.Empty;
        
        [BsonElement("details")]
        public string Details { get; set; } = string.Empty;
        
        [BsonElement("previousStatus")]
        public PaymentRequestStatus? PreviousStatus { get; set; }
        
        [BsonElement("newStatus")]
        public PaymentRequestStatus? NewStatus { get; set; }
    }
    
    public class AdminComment
    {
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [BsonElement("adminId")]
        public string AdminId { get; set; } = string.Empty;
        
        [BsonElement("adminName")]
        public string AdminName { get; set; } = string.Empty;
        
        [BsonElement("comment")]
        public string Comment { get; set; } = string.Empty;
        
        [BsonElement("isInternal")]
        public bool IsInternal { get; set; } = false;
    }
} 