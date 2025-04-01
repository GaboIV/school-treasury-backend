using System;
using Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace SchoolTreasureAPI.Domain.Entities
{
    public class DownloadStat : BaseEntity
    {
        [BsonElement("appVersionId")]
        public string AppVersionId { get; set; }
        
        [BsonElement("version")]
        public string Version { get; set; }
        
        [BsonElement("deviceModel")]
        public string DeviceModel { get; set; }
        
        [BsonElement("deviceOs")]
        public string DeviceOs { get; set; }
        
        [BsonElement("deviceOsVersion")]
        public string DeviceOsVersion { get; set; }
        
        [BsonElement("ipAddress")]
        public string IpAddress { get; set; }
        
        [BsonElement("country")]
        public string Country { get; set; }
        
        [BsonElement("city")]
        public string City { get; set; }
        
        [BsonElement("downloadDate")]
        public DateTime DownloadDate { get; set; }
        
        [BsonElement("isUpdate")]
        public bool IsUpdate { get; set; }
        
        [BsonElement("previousVersion")]
        public string PreviousVersion { get; set; }
    }
} 