using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IMongoCollection<Transaction> _collection;

        public TransactionRepository(MongoDbContext context)
        {
            _collection = context.Transactions;
        }

        public async Task<List<Transaction>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Transaction> GetByIdAsync(string id)
        {
            return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Transaction> CreateAsync(Transaction transaction)
        {
            transaction.Date = DateTime.UtcNow;
            await _collection.InsertOneAsync(transaction);
            return transaction;
        }

        public async Task<List<Transaction>> GetPaginatedAsync(int page, int pageSize)
        {
            return await _collection.Find(_ => true)
                .Sort(Builders<Transaction>.Sort.Descending(t => t.Date))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetByRelatedEntityAsync(string relatedEntityId, string relatedEntityType)
        {
            return await _collection.Find(t => 
                t.RelatedEntityId == relatedEntityId && 
                t.RelatedEntityType == relatedEntityType)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return (int)await _collection.CountDocumentsAsync(_ => true);
        }
        
        public async Task<List<Transaction>> GetAllOrderedByDateAsync()
        {
            return await _collection.Find(_ => true)
                .Sort(Builders<Transaction>.Sort.Ascending(t => t.Date))
                .ToListAsync();
        }
        
        public async Task<bool> UpdateAsync(string id, Transaction transaction)
        {
            transaction.UpdatedAt = DateTime.UtcNow;
            var result = await _collection.ReplaceOneAsync(t => t.Id == id, transaction);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
} 