using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly IMongoCollection<Expense> _expensives;

        public ExpenseRepository(IMongoDatabase database)
        {
            _expensives = database.GetCollection<Expense>("Expenses");
        }

        public async Task<List<Expense>> GetAllAsync() =>
            await _expensives.Find(_ => true).ToListAsync();

        public async Task<Expense> GetByIdAsync(string id) =>
            await _expensives.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task InsertAsync(Expense expensive) =>
            await _expensives.InsertOneAsync(expensive);

        public async Task UpdateAsync(Expense expensive)
        {
            var filter = Builders<Expense>.Filter.Eq(c => c.Id, expensive.Id);
            var update = Builders<Expense>.Update
                .Set(c => c.Name, expensive.Name)
                .Set(c => c.Date, expensive.Date);
                
            await _expensives.UpdateOneAsync(filter, update);
        }
    }
}
