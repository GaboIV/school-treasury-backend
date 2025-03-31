using System.Threading.Tasks;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.Application.Interfaces
{
    public interface IAppInfoRepository
    {
        Task<AppInfo> GetAppInfoAsync();
        Task<AppInfo> UpdateAppInfoAsync(AppInfo appInfo);
        Task<bool> CreateAppInfoAsync(AppInfo appInfo);
    }
} 