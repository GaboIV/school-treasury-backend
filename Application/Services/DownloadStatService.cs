using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchoolTreasureAPI.Application.DTOs;
using SchoolTreasureAPI.Application.Interfaces;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.Application.Services
{
    public class DownloadStatService : IDownloadStatService
    {
        private readonly IDownloadStatRepository _downloadStatRepository;

        public DownloadStatService(IDownloadStatRepository downloadStatRepository)
        {
            _downloadStatRepository = downloadStatRepository;
        }

        public async Task<DownloadStat> RecordDownloadAsync(string versionId, string version, DownloadStatDTO statDTO, string ipAddress)
        {
            var downloadStat = new DownloadStat
            {
                AppVersionId = versionId,
                Version = version,
                DeviceModel = statDTO.DeviceModel,
                DeviceOs = statDTO.DeviceOs,
                DeviceOsVersion = statDTO.DeviceOsVersion,
                IpAddress = ipAddress,
                Country = "", // Podría implementarse usando un servicio de geolocalización por IP
                City = "",
                DownloadDate = DateTime.UtcNow,
                IsUpdate = statDTO.IsUpdate,
                PreviousVersion = statDTO.PreviousVersion
            };

            return await _downloadStatRepository.AddDownloadStatAsync(downloadStat);
        }

        public async Task<StatsResponseDTO> GetStatsAsync()
        {
            var allStats = await _downloadStatRepository.GetAllStatsAsync();
            return await GenerateStatsResponse(allStats);
        }

        public async Task<StatsResponseDTO> GetStatsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var stats = await _downloadStatRepository.GetStatsByDateRangeAsync(startDate, endDate);
            return await GenerateStatsResponse(stats);
        }

        public async Task<StatsResponseDTO> GetStatsByVersionAsync(string version)
        {
            var stats = await _downloadStatRepository.GetStatsByVersionAsync(version);
            return await GenerateStatsResponse(stats);
        }

        private async Task<StatsResponseDTO> GenerateStatsResponse(IEnumerable<DownloadStat> stats)
        {
            var totalDownloads = await _downloadStatRepository.GetTotalDownloadsAsync();
            var totalUpdates = await _downloadStatRepository.GetTotalUpdatesAsync();
            var uniqueDevices = await _downloadStatRepository.GetUniqueDevicesAsync();

            var statsList = stats.ToList();
            
            // Agregar estadísticas diarias
            var dailyStats = statsList
                .GroupBy(s => s.DownloadDate.Date)
                .Select(g => new DailyStatDTO
                {
                    Date = g.Key,
                    Downloads = g.Count(),
                    Updates = g.Count(s => s.IsUpdate)
                })
                .OrderBy(s => s.Date)
                .ToList();

            // Estadísticas por versión
            var versionStats = statsList
                .GroupBy(s => s.Version)
                .Select(g => new VersionStatDTO
                {
                    Version = g.Key,
                    Downloads = g.Count(),
                    Percentage = Math.Round((double)g.Count() / totalDownloads * 100, 2)
                })
                .OrderByDescending(s => s.Downloads)
                .ToList();

            // Estadísticas por dispositivo
            var deviceStats = statsList
                .GroupBy(s => s.DeviceOs)
                .Select(g => new DeviceStatDTO
                {
                    DeviceOs = g.Key,
                    Count = g.Count(),
                    Percentage = Math.Round((double)g.Count() / totalDownloads * 100, 2)
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            return new StatsResponseDTO
            {
                TotalDownloads = totalDownloads,
                TotalUpdates = totalUpdates,
                UniqueDevices = uniqueDevices,
                DailyStats = dailyStats,
                VersionStats = versionStats,
                DeviceStats = deviceStats
            };
        }
    }
} 