using System;
using Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace SchoolTreasureAPI.Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class AppInfo : BaseEntity
    {
        [BsonElement("appName")]
        public string? AppName { get; set; }

        [BsonElement("packageName")]
        public string? PackageName { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("logoUrl")]
        public string? LogoUrl { get; set; }

        [BsonElement("developer")]
        public string? Developer { get; set; }

        [BsonElement("contactEmail")]
        public string? ContactEmail { get; set; }

        [BsonElement("websiteUrl")]
        public string? WebsiteUrl { get; set; }

        [BsonElement("privacyPolicyUrl")]
        public string? PrivacyPolicyUrl { get; set; }

        [BsonElement("lastUpdate")]
        public DateTime LastUpdate { get; set; }
        
        // Nuevos campos basados en el frontend
        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }
        
        [BsonElement("name")]
        public string? Name { get; set; }
        
        [BsonElement("category")]
        public string? Category { get; set; }
        
        [BsonElement("version")]
        public string? Version { get; set; }
        
        [BsonElement("downloads")]
        public int Downloads { get; set; }
        
        [BsonElement("size")]
        public double Size { get; set; }
        
        [BsonElement("releaseDate")]
        public DateTime ReleaseDate { get; set; }
        
        [BsonElement("minAndroidVersion")]
        public string? MinAndroidVersion { get; set; }
        
        [BsonElement("rating")]
        public int Rating { get; set; }
        
        // Campos para el changelog
        [BsonElement("changeLog")]
        public List<string>? ChangeLog { get; set; }
    }
}