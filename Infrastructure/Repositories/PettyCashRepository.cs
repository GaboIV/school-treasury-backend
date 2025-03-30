using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PettyCashRepository : IPettyCashRepository
    {
        private readonly IMongoCollection<PettyCash> _collection;

        public PettyCashRepository(MongoDbContext context)
        {
            _collection = context.PettyCash;
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
            else if (type == TransactionType.Expense)
            {
                pettyCash.TotalExpense += amount;
                pettyCash.CurrentBalance -= amount;
            }
            else if (type == TransactionType.Collection)
            {
                pettyCash.TotalExpense += amount;
                pettyCash.CurrentBalance -= amount;
            }
            else if (type == TransactionType.Exonerated)
            {
                // Las transacciones de exoneración no afectan el balance de caja chica
                // Solo se registran para llevar un historial
            }

            await UpdateAsync(pettyCash);
        }
    }
} 