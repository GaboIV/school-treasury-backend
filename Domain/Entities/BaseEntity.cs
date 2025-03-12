using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Domain.Entities
{
    public class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("status")]
        public bool? Status { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("deletedAt")]
        public DateTime? DeletedAt { get; set; } = null;
    }
}