using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class StudentPayment : BaseEntity
    {
        [BsonElement("collectionId")]
        public string CollectionId { get; set; } = string.Empty;
        
        [BsonElement("studentId")]
        public string StudentId { get; set; } = string.Empty;
        
        [BsonElement("amountCollection")]
        public decimal AmountCollection { get; set; }
        
        [BsonElement("amountPaid")]
        public decimal AmountPaid { get; set; } = 0;
        
        [BsonElement("paymentStatus")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        
        [BsonElement("images")]
        public List<string> Images { get; set; } = new List<string>();
        
        [BsonElement("voucher")]
        public string? Voucher { get; set; }
        

        
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
        Exonerated
    }
} 