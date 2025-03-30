using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.Application.Interfaces
{
    public interface IAppVersionService
    {
        Task<AppVersion> GetLatestAvailableVersionAsync();
        Task<IEnumerable<AppVersion>> GetAllVersionsAsync();
        Task<AppVersion> GetVersionByIdAsync(int id);
        Task<AppVersion> GetVersionByIdAsync(string id);
        Task<AppVersion> AddVersionAsync(AppVersion version);
        Task<AppVersion> UpdateVersionAsync(AppVersion version);
        Task<bool> DeleteVersionAsync(int id);
        Task<bool> DeleteVersionAsync(string id);
        Task<bool> IsUpdateAvailableAsync(string currentVersion);
    }
} 