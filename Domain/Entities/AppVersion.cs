using System;
using Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace SchoolTreasureAPI.Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class AppVersion : BaseEntity
    {
        [BsonElement("version")]
        public string Version { get; set; }

        [BsonElement("changeLog")]
        public string ChangeLog { get; set; }

        [BsonElement("apkFilename")]
        public string ApkFilename { get; set; }

        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; }

        [BsonElement("isRequired")]
        public bool IsRequired { get; set; }

        [BsonElement("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        [BsonElement("downloadUrl")]
        public string DownloadUrl { get; set; }
    }
}