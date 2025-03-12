using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class PettyCash : BaseEntity
    {
        [BsonElement("currentBalance")]
        public decimal CurrentBalance { get; set; } = 0;

        [BsonElement("totalIncome")]
        public decimal TotalIncome { get; set; } = 0;

        [BsonElement("totalExpense")]
        public decimal TotalExpense { get; set; } = 0;

        [BsonElement("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
} 