using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchoolTreasureAPI.Application.Interfaces;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.Application.Services
{
    public class AppVersionService : IAppVersionService
    {
        private readonly IAppVersionRepository _appVersionRepository;

        public AppVersionService(IAppVersionRepository appVersionRepository)
        {
            _appVersionRepository = appVersionRepository;
        }

        public async Task<AppVersion> GetLatestAvailableVersionAsync()
        {
            return await _appVersionRepository.GetLatestAvailableVersionAsync();
        }

        public async Task<IEnumerable<AppVersion>> GetAllVersionsAsync()
        {
            return await _appVersionRepository.GetAllVersionsAsync();
        }

        public async Task<AppVersion> GetVersionByIdAsync(int id)
        {
            return await _appVersionRepository.GetVersionByIdAsync(id);
        }

        public async Task<AppVersion> GetVersionByIdAsync(string id)
        {
            return await _appVersionRepository.GetVersionByIdAsync(id);
        }

        public async Task<AppVersion> AddVersionAsync(AppVersion version)
        {
            return await _appVersionRepository.AddVersionAsync(version);
        }

        public async Task<AppVersion> UpdateVersionAsync(AppVersion version)
        {
            return await _appVersionRepository.UpdateVersionAsync(version);
        }

        public async Task<bool> DeleteVersionAsync(int id)
        {
            return await _appVersionRepository.DeleteVersionAsync(id);
        }

        public async Task<bool> DeleteVersionAsync(string id)
        {
            return await _appVersionRepository.DeleteVersionAsync(id);
        }

        public async Task<bool> IsUpdateAvailableAsync(string currentVersion)
        {
            var latestVersion = await GetLatestAvailableVersionAsync();
            
            if (latestVersion == null)
                return false;
                
            return CompareVersions(latestVersion.Version, currentVersion) > 0;
        }

        private int CompareVersions(string version1, string version2)
        {
            var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var v2Parts = version2.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
            {
                var v1 = i < v1Parts.Length ? v1Parts[i] : 0;
                var v2 = i < v2Parts.Length ? v2Parts[i] : 0;

                if (v1 > v2) return 1;
                if (v1 < v2) return -1;
            }

            return 0;
        }
    }
} 