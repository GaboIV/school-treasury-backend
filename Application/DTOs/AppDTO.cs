using System;
using System.Collections.Generic;

namespace SchoolTreasureAPI.Application.DTOs
{
    // App Info DTOs
    public class AppInfoDTO
    {
        public string Id { get; set; }
        public string AppName { get; set; }
        public string PackageName { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string Developer { get; set; }
        public string ContactEmail { get; set; }
        public string WebsiteUrl { get; set; }
        public string PrivacyPolicyUrl { get; set; }
        public DateTime LastUpdate { get; set; }
        public AppVersionResponseDTO LatestVersion { get; set; }
        
        // Nuevos campos para el frontend
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public int Downloads { get; set; }
        public double Size { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string MinAndroidVersion { get; set; }
        public int Rating { get; set; }
        public List<string> ChangeLog { get; set; }
    }

    // Stats DTOs
    public class DownloadStatDTO
    {
        public string DeviceModel { get; set; }
        public string DeviceOs { get; set; }
        public string DeviceOsVersion { get; set; }
        public string PreviousVersion { get; set; }
        public bool IsUpdate { get; set; }
    }

    public class StatsResponseDTO
    {
        public int TotalDownloads { get; set; }
        public int TotalUpdates { get; set; }
        public int UniqueDevices { get; set; }
        public List<DailyStatDTO> DailyStats { get; set; }
        public List<VersionStatDTO> VersionStats { get; set; }
        public List<DeviceStatDTO> DeviceStats { get; set; }
    }

    public class DailyStatDTO
    {
        public DateTime Date { get; set; }
        public int Downloads { get; set; }
        public int Updates { get; set; }
    }

    public class VersionStatDTO
    {
        public string Version { get; set; }
        public int Downloads { get; set; }
        public double Percentage { get; set; }
    }

    public class DeviceStatDTO
    {
        public string DeviceOs { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    // Update response
    public class CheckUpdateResponseDTO
    {
        public bool IsUpdateAvailable { get; set; }
        public bool IsRequired { get; set; }
        public string LatestVersion { get; set; }
        public string ChangeLog { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string DownloadUrl { get; set; }
    }
} 