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
            
            // Utilizamos ReplaceOneAsync para reemplazar todo el documento
            // Esto evita tener que especificar cada campo a actualizar
            await _expensiveTypes.ReplaceOneAsync(filter, expensiveType);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _expensiveTypes.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<(List<ExpenseType> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            var totalCount = await _expensiveTypes.CountDocumentsAsync(_ => true);
            
            var items = await _expensiveTypes.Find(_ => true)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
            
            return (items, (int)totalCount);
        }
    }
}
