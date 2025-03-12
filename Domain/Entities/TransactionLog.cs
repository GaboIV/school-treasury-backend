using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Domain.Entities
{
    public class TransactionLog : BaseEntity
    {
        [BsonElement("transactionId")]
        public string TransactionId { get; set; } = string.Empty;

        [BsonElement("date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [BsonElement("type")]
        public TransactionType Type { get; set; }

        [BsonElement("amount")]
        public decimal Amount { get; set; }

        [BsonElement("balanceBefore")]
        public decimal BalanceBefore { get; set; }

        [BsonElement("balanceAfter")]
        public decimal BalanceAfter { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("relatedEntityId")]
        public string? RelatedEntityId { get; set; }

        [BsonElement("relatedEntityType")]
        public string? RelatedEntityType { get; set; }

        [BsonElement("userId")]
        public string? UserId { get; set; }

        [BsonElement("userName")]
        public string? UserName { get; set; }

        [BsonElement("ipAddress")]
        public string? IpAddress { get; set; }
    }
} 