using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Domain.Entities
{
    public class Transaction : BaseEntity
    {
        [BsonElement("date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [BsonElement("type")]
        public TransactionType Type { get; set; }

        [BsonElement("amount")]
        public decimal Amount { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("relatedEntityId")]
        public string? RelatedEntityId { get; set; }

        [BsonElement("relatedEntityType")]
        public string? RelatedEntityType { get; set; }
        
        // Información adicional del estudiante
        [BsonElement("studentId")]
        public string? StudentId { get; set; }
        
        [BsonElement("studentName")]
        public string? StudentName { get; set; }
        
        // Información adicional del pago
        [BsonElement("collectionId")]
        public string? CollectionId { get; set; }
        
        [BsonElement("collectionName")]
        public string? CollectionName { get; set; }
        
        [BsonElement("paymentId")]
        public string? PaymentId { get; set; }
        
        [BsonElement("paymentStatus")]
        public string? PaymentStatus { get; set; }
        
        // Información adicional para el seguimiento
        [BsonElement("createdBy")]
        public string? CreatedBy { get; set; }
        
        [BsonElement("notes")]
        public string? Notes { get; set; }

        // Saldos de caja chica
        [BsonElement("previousBalance")]
        public decimal PreviousBalance { get; set; }
        
        [BsonElement("newBalance")]
        public decimal NewBalance { get; set; }
    }

    public enum TransactionType
    {
        Income,     // Ingreso
        Expense,    // Gasto
        Collection, // Cobro
        Exonerated  // Exonerado
    }
} 