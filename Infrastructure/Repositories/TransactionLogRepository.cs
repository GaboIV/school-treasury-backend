using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TransactionLogRepository : ITransactionLogRepository
    {
        private readonly IMongoCollection<TransactionLog> _collection;

        public TransactionLogRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<TransactionLog>("TransactionLogs");
        }

        public async Task<IEnumerable<TransactionLog>> GetAllAsync()
        {
            return await _collection.Find(log => true).ToListAsync();
        }

        public async Task<TransactionLog> GetByIdAsync(string id)
        {
            return await _collection.Find(log => log.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TransactionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _collection.Find(log => log.Date >= startDate && log.Date <= endDate)
                                  .SortByDescending(log => log.Date)
                                  .ToListAsync();
        }

        public async Task<IEnumerable<TransactionLog>> GetByRelatedEntityAsync(string relatedEntityId, string relatedEntityType)
        {
            return await _collection.Find(log => log.RelatedEntityId == relatedEntityId && log.RelatedEntityType == relatedEntityType)
                                  .SortByDescending(log => log.Date)
                                  .ToListAsync();
        }

        public async Task<TransactionLog> AddAsync(TransactionLog log)
        {
            log.CreatedAt = DateTime.UtcNow;
            log.UpdatedAt = DateTime.UtcNow;
            log.Status = true;
            await _collection.InsertOneAsync(log);
            return log;
        }

        public async Task<(IEnumerable<TransactionLog> Logs, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            var totalCount = await _collection.CountDocumentsAsync(log => true);
            
            var logs = await _collection.Find(log => true)
                                      .SortByDescending(log => log.Date)
                                      .Skip((page - 1) * pageSize)
                                      .Limit(pageSize)
                                      .ToListAsync();
            
            return (logs, (int)totalCount);
        }
    }
} 