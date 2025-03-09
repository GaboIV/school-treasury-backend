using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities {
    public class Expense : BaseEntity
    {
        [BsonElement("expenseTypeId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ExpenseTypeId { get; set; }

        [BsonElement("expenseType")]
        [BsonIgnoreIfNull]
        public ExpenseType? ExpenseType { get; set; }

        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("individualAmount")]
        public decimal IndividualAmount { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("percetagePaid")]
        public decimal PercentagePaid { get; set; }

        [BsonElement("advance")]
        public Advance Advance { get; set; } = new Advance();

        [BsonElement("studentQuantity")]
        public string? StudentQuantity { get; set; } = "all";

        [BsonElement("images")]
        public List<Image> Images { get; set; } = new List<Image>();
    }

    public class Advance
    {
        [BsonElement("total")]
        public int Total { get; set; }

        [BsonElement("completed")]
        public int Completed { get; set; }

        [BsonElement("pending")]
        public int Pending { get; set; }
    }

    public class Image
    {
        public string Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}

