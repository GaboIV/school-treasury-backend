using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class CollectionType : BaseEntity
    {
        [BsonElement("name")]
        public required string Name { get; set; }
    }
}