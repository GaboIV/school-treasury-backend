using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities {
    public class Collection : BaseEntity
    {
        [BsonElement("collectionTypeId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CollectionTypeId { get; set; }

        [BsonElement("collectionType")]
        [BsonIgnoreIfNull]
        public CollectionType? CollectionType { get; set; }

        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("individualAmount")]
        public decimal IndividualAmount { get; set; }

        [BsonElement("adjustedIndividualAmount")]
        public decimal? AdjustedIndividualAmount { get; set; }

        [BsonElement("totalSurplus")]
        public decimal TotalSurplus { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("percetagePaid")]
        public decimal PercentagePaid { get; set; }

        [BsonElement("allowsExemptions")]
        public bool? AllowsExemptions { get; set; } = false;

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

