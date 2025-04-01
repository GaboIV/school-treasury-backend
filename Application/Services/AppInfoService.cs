using System.Threading.Tasks;
using SchoolTreasureAPI.Application.DTOs;
using SchoolTreasureAPI.Application.Interfaces;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.Application.Services
{
    public class AppInfoService : IAppInfoService
    {
        private readonly IAppInfoRepository _appInfoRepository;
        private readonly IAppVersionService _appVersionService;

        public AppInfoService(IAppInfoRepository appInfoRepository, IAppVersionService appVersionService)
        {
            _appInfoRepository = appInfoRepository;
            _appVersionService = appVersionService;
        }

        public async Task<AppInfoDTO> GetAppInfoAsync()
        {
            var appInfo = await _appInfoRepository.GetAppInfoAsync();

            if (appInfo == null)
            {
                return null;
            }

            var latestVersion = await _appVersionService.GetLatestAvailableVersionAsync();

            return new AppInfoDTO
            {
                Id = appInfo.Id,
                AppName = appInfo.AppName,
                PackageName = appInfo.PackageName,
                Description = appInfo.Description,
                LogoUrl = appInfo.LogoUrl,
                Developer = appInfo.Developer,
                ContactEmail = appInfo.ContactEmail,
                WebsiteUrl = appInfo.WebsiteUrl,
                PrivacyPolicyUrl = appInfo.PrivacyPolicyUrl,
                LastUpdate = appInfo.LastUpdate,

                // Nuevos campos para el frontend
                ImageUrl = appInfo.ImageUrl,
                Name = appInfo.Name ?? appInfo.AppName,  // Usar AppName como fallback si Name es null
                Category = appInfo.Category,
                Version = latestVersion?.Version ?? appInfo.Version,
                Downloads = appInfo.Downloads,
                Size = appInfo.Size,
                ReleaseDate = latestVersion?.ReleaseDate ?? appInfo.ReleaseDate,
                MinAndroidVersion = appInfo.MinAndroidVersion,
                Rating = appInfo.Rating,
                ChangeLog = appInfo.ChangeLog,

                LatestVersion = latestVersion != null ? new AppVersionResponseDTO
                {
                    Id = latestVersion.Id,
                    Version = latestVersion.Version,
                    ChangeLog = latestVersion.ChangeLog,
                    ApkFilename = latestVersion.ApkFilename,
                    IsAvailable = latestVersion.IsAvailable,
                    IsRequired = latestVersion.IsRequired,
                    ReleaseDate = latestVersion.ReleaseDate,
                    DownloadUrl = latestVersion.DownloadUrl
                } : null
            };
        }

        public async Task<AppInfo> UpdateAppInfoAsync(AppInfo appInfo)
        {
            return await _appInfoRepository.UpdateAppInfoAsync(appInfo);
        }

        public async Task<bool> CreateAppInfoAsync(AppInfo appInfo)
        {
            return await _appInfoRepository.CreateAppInfoAsync(appInfo);
        }

        public async Task<CheckUpdateResponseDTO> CheckForUpdateAsync(string currentVersion, string baseUrl)
        {
            var isUpdateAvailable = await _appVersionService.IsUpdateAvailableAsync(currentVersion);

            if (!isUpdateAvailable)
            {
                return new CheckUpdateResponseDTO
                {
                    IsUpdateAvailable = false
                };
            }

            var latestVersion = await _appVersionService.GetLatestAvailableVersionAsync();

            // Usar la URL de descarga almacenada en la entidad si existe
            string downloadUrl = !string.IsNullOrEmpty(latestVersion.DownloadUrl)
                ? latestVersion.DownloadUrl
                : $"{baseUrl}/api/apps/download/latest";

            return new CheckUpdateResponseDTO
            {
                IsUpdateAvailable = true,
                IsRequired = latestVersion.IsRequired,
                LatestVersion = latestVersion.Version,
                ChangeLog = latestVersion.ChangeLog,
                ReleaseDate = latestVersion.ReleaseDate,
                DownloadUrl = downloadUrl
            };
        }
    }
}