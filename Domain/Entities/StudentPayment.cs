using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class StudentPayment : BaseEntity
    {
        [BsonElement("expenseId")]
        public string ExpenseId { get; set; } = string.Empty;
        
        [BsonElement("studentId")]
        public string StudentId { get; set; } = string.Empty;
        
        [BsonElement("amountExpense")]
        public decimal AmountExpense { get; set; }
        
        [BsonElement("adjustedAmountExpense")]
        public decimal AdjustedAmountExpense { get; set; }
        
        [BsonElement("amountPaid")]
        public decimal AmountPaid { get; set; } = 0;
        
        [BsonElement("paymentStatus")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        
        [BsonElement("images")]
        public List<string> Images { get; set; } = new List<string>();
        
        [BsonElement("voucher")]
        public string? Voucher { get; set; }
        
        [BsonElement("excedent")]
        public decimal Excedent { get; set; } = 0;
        
        [BsonElement("surplus")]
        public decimal Surplus { get; set; } = 0;
        
        [BsonElement("pending")]
        public decimal Pending { get; set; }
        
        [BsonElement("comment")]
        public string? Comment { get; set; }
        
        [BsonElement("paymentDate")]
        public DateTime? PaymentDate { get; set; }
    }
    
    public enum PaymentStatus
    {
        Pending,
        PartiallyPaid,
        Paid,
        Excedent
    }
} 