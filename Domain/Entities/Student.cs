using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class Student : BaseEntity
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
    }
} 