using System;
using Domain.Entities;

namespace SchoolTreasureAPI.Domain.Entities
{
    public class AppVersion : BaseEntity
    {
        public string Version { get; set; }
        public string ChangeLog { get; set; }
        public string ApkFilename { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsRequired { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
} 