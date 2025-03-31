using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using MongoDB.Driver;
using SchoolTreasureAPI.Application.Interfaces;
using SchoolTreasureAPI.Domain.Entities;

namespace Infrastructure.Repositories
{
    public class DownloadStatRepository : IDownloadStatRepository
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<DownloadStat> _downloadStats;

        public DownloadStatRepository(MongoDbContext context)
        {
            _context = context;
            _downloadStats = context.DownloadStats;
        }

        public async Task<DownloadStat> AddDownloadStatAsync(DownloadStat stat)
        {
            await _downloadStats.InsertOneAsync(stat);
            return stat;
        }

        public async Task<IEnumerable<DownloadStat>> GetAllStatsAsync()
        {
            return await _downloadStats.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<DownloadStat>> GetStatsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _downloadStats
                .Find(s => s.DownloadDate >= startDate && s.DownloadDate <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<DownloadStat>> GetStatsByVersionAsync(string version)
        {
            return await _downloadStats
                .Find(s => s.Version == version)
                .ToListAsync();
        }

        public async Task<int> GetTotalDownloadsAsync()
        {
            return (int)await _downloadStats.CountDocumentsAsync(_ => true);
        }

        public async Task<int> GetTotalUpdatesAsync()
        {
            return (int)await _downloadStats.CountDocumentsAsync(s => s.IsUpdate);
        }

        public async Task<int> GetUniqueDevicesAsync()
        {
            var result = await _downloadStats.Aggregate()
                .Group(x => new { x.DeviceModel, x.DeviceOs }, g => new { DeviceKey = g.Key, Count = g.Count() })
                .ToListAsync();

            return result.Count;
        }
    }
} 