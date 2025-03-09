using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class ExpenseTypeRepository : IExpenseTypeRepository
    {
        private readonly IMongoCollection<ExpenseType> _expensiveTypes;

        public ExpenseTypeRepository(IMongoDatabase database)
        {
            _expensiveTypes = database.GetCollection<ExpenseType>("ExpenseTypes");
        }

        public async Task<List<ExpenseType>> GetAllAsync() =>
            await _expensiveTypes.Find(_ => true).ToListAsync();

        public async Task<ExpenseType> GetByIdAsync(string id) =>
            await _expensiveTypes.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task InsertAsync(ExpenseType expensiveType) =>
            await _expensiveTypes.InsertOneAsync(expensiveType);

        public async Task UpdateAsync(ExpenseType expensiveType)
        {
            var filter = Builders<ExpenseType>.Filter.Eq(c => c.Id, expensiveType.Id);
            var update = Builders<ExpenseType>.Update
                .Set(c => c.Name, expensiveType.Name);
                
            await _expensiveTypes.UpdateOneAsync(filter, update);
        }
    }
}
