using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class Student : BaseEntity
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("avatar")]
        public string Avatar { get; set; } = "001-boy.svg";
    }
} 