using System.Threading.Tasks;
using Infrastructure.Persistence;
using MongoDB.Driver;
using SchoolTreasureAPI.Application.Interfaces;
using SchoolTreasureAPI.Domain.Entities;

namespace Infrastructure.Repositories
{
    public class AppInfoRepository : IAppInfoRepository
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<AppInfo> _appInfo;

        public AppInfoRepository(MongoDbContext context)
        {
            _context = context;
            _appInfo = context.AppInfo;
        }

        public async Task<AppInfo> GetAppInfoAsync()
        {
            // Siempre devolvemos el primer documento, ya que solo hay uno
            return await _appInfo.Find(_ => true).FirstOrDefaultAsync();
        }

        public async Task<AppInfo> UpdateAppInfoAsync(AppInfo appInfo)
        {
            await _appInfo.ReplaceOneAsync(a => a.Id == appInfo.Id, appInfo);
            return appInfo;
        }

        public async Task<bool> CreateAppInfoAsync(AppInfo appInfo)
        {
            // Verificar si ya existe un registro
            var existingAppInfo = await GetAppInfoAsync();
            
            if (existingAppInfo != null)
            {
                return false; // No se puede crear uno nuevo si ya existe
            }
            
            await _appInfo.InsertOneAsync(appInfo);
            return true;
        }
    }
} 