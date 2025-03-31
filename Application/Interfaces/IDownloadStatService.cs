using System;
using System.Threading.Tasks;
using SchoolTreasureAPI.Application.DTOs;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.Application.Interfaces
{
    public interface IDownloadStatService
    {
        Task<DownloadStat> RecordDownloadAsync(string versionId, string version, DownloadStatDTO statDTO, string ipAddress);
        Task<StatsResponseDTO> GetStatsAsync();
        Task<StatsResponseDTO> GetStatsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<StatsResponseDTO> GetStatsByVersionAsync(string version);
    }
} 