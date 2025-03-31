using System.Threading.Tasks;
using SchoolTreasureAPI.Application.DTOs;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.Application.Interfaces
{
    public interface IAppInfoService
    {
        Task<AppInfoDTO> GetAppInfoAsync();
        Task<AppInfo> UpdateAppInfoAsync(AppInfo appInfo);
        Task<bool> CreateAppInfoAsync(AppInfo appInfo);
        Task<CheckUpdateResponseDTO> CheckForUpdateAsync(string currentVersion, string baseUrl);
    }
} 