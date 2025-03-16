using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class InterestLink : BaseEntity
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        
        [BsonElement("url")]
        public string Url { get; set; } = string.Empty;
        
        [BsonElement("description")]
        public string? Description { get; set; }
        
        [BsonElement("order")]
        public int Order { get; set; } = 0;
    }
} 