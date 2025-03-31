using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.Application.Interfaces
{
    public interface IDownloadStatRepository
    {
        Task<DownloadStat> AddDownloadStatAsync(DownloadStat stat);
        Task<IEnumerable<DownloadStat>> GetAllStatsAsync();
        Task<IEnumerable<DownloadStat>> GetStatsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<DownloadStat>> GetStatsByVersionAsync(string version);
        Task<int> GetTotalDownloadsAsync();
        Task<int> GetTotalUpdatesAsync();
        Task<int> GetUniqueDevicesAsync();
    }
} 