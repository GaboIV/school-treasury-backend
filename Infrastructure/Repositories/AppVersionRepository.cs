using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using MongoDB.Driver;
using SchoolTreasureAPI.Application.Interfaces;
using SchoolTreasureAPI.Domain.Entities;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class AppVersionRepository : IAppVersionRepository
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<AppVersion> _appVersions;

        public AppVersionRepository(MongoDbContext context)
        {
            _context = context;
            _appVersions = context.AppVersions;
        }

        public async Task<AppVersion> GetLatestAvailableVersionAsync()
        {
            return await _appVersions
                .Find(v => v.IsAvailable)
                .SortByDescending(v => v.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppVersion>> GetAllVersionsAsync()
        {
            return await _appVersions
                .Find(_ => true)
                .SortByDescending(v => v.Version)
                .ToListAsync();
        }

        public async Task<AppVersion> GetVersionByIdAsync(int id)
        {
            return await _appVersions
                .Find(v => v.Id == id.ToString())
                .FirstOrDefaultAsync();
        }

        public async Task<AppVersion> GetVersionByIdAsync(string id)
        {
            return await _appVersions
                .Find(v => v.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<AppVersion> AddVersionAsync(AppVersion version)
        {
            await _appVersions.InsertOneAsync(version);
            return version;
        }

        public async Task<AppVersion> UpdateVersionAsync(AppVersion version)
        {
            await _appVersions.ReplaceOneAsync(v => v.Id == version.Id, version);
            return version;
        }

        public async Task<bool> DeleteVersionAsync(int id)
        {
            var result = await _appVersions.DeleteOneAsync(v => v.Id == id.ToString());
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteVersionAsync(string id)
        {
            var result = await _appVersions.DeleteOneAsync(v => v.Id == id);
            return result.DeletedCount > 0;
        }
    }
} 