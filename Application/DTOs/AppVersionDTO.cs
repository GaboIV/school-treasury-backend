using System;
using Microsoft.AspNetCore.Http;

namespace SchoolTreasureAPI.Application.DTOs
{
    public class AppVersionDTO
    {
        public string Version { get; set; }
        public string ChangeLog { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsRequired { get; set; }
    }

    public class AppVersionUploadDTO : AppVersionDTO
    {
        public IFormFile ApkFile { get; set; }
    }

    public class AppVersionResponseDTO : AppVersionDTO
    {
        public string Id { get; set; }
        public string ApkFilename { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
} 