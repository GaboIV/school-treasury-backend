using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PettyCashRepository : IPettyCashRepository
    {
        private readonly IMongoCollection<PettyCash> _collection;

        public PettyCashRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<PettyCash>("PettyCash");
        }

        public async Task<PettyCash> GetAsync()
        {
            return await _collection.Find(_ => true).FirstOrDefaultAsync();
        }

        public async Task<PettyCash> CreateAsync(PettyCash pettyCash)
        {
            await _collection.InsertOneAsync(pettyCash);
            return pettyCash;
        }

        public async Task<PettyCash> UpdateAsync(PettyCash pettyCash)
        {
            pettyCash.UpdatedAt = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(p => p.Id == pettyCash.Id, pettyCash);
            return pettyCash;
        }

        public async Task<bool> ExistsAsync()
        {
            return await _collection.Find(_ => true).AnyAsync();
        }

        public async Task UpdateBalanceAsync(decimal amount, TransactionType type)
        {
            var pettyCash = await GetAsync();
            if (pettyCash == null)
            {
                pettyCash = new PettyCash();
                await CreateAsync(pettyCash);
            }

            pettyCash.LastUpdated = DateTime.UtcNow;

            if (type == TransactionType.Income)
            {
                pettyCash.TotalIncome += amount;
                pettyCash.CurrentBalance += amount;
            }
            else
            {
                pettyCash.TotalExpense += amount;
                pettyCash.CurrentBalance -= amount;
            }

            await UpdateAsync(pettyCash);
        }
    }
} 