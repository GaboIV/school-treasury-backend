using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly IMongoCollection<Expense> _expenses;

        public ExpenseRepository(IMongoDatabase database)
        {
            _expenses = database.GetCollection<Expense>("Expenses");
        }

        public async Task<List<Expense>> GetAllAsync() =>
            await _expenses.Find(_ => true).ToListAsync();

        public async Task<Expense> GetByIdAsync(string id) =>
            await _expenses.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task InsertAsync(Expense expensive) =>
            await _expenses.InsertOneAsync(expensive);

        public async Task UpdateAsync(Expense expensive)
        {
            var filter = Builders<Expense>.Filter.Eq(c => c.Id, expensive.Id);
            
            // Utilizamos ReplaceOneAsync para reemplazar todo el documento
            // Esto evita tener que especificar cada campo a actualizar
            await _expenses.ReplaceOneAsync(filter, expensive);
        }

        public async Task<bool> ExistsByExpenseTypeIdAsync(string expenseTypeId)
        {
            var count = await _expenses.CountDocumentsAsync(c => c.ExpenseTypeId == expenseTypeId);
            return count > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _expenses.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<(List<Expense> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            var query = from expense in _expenses.AsQueryable()
                        join expenseType in _expenses.Database.GetCollection<ExpenseType>("ExpenseTypes").AsQueryable()
                        on expense.ExpenseTypeId equals expenseType.Id into expenseGroup
                        from expenseType in expenseGroup.DefaultIfEmpty()
                        select new Expense
                        {
                            Id = expense.Id,
                            ExpenseTypeId = expense.ExpenseTypeId,
                            Name = expense.Name,
                            TotalAmount = expense.TotalAmount,
                            IndividualAmount = expense.IndividualAmount,
                            Date = expense.Date,
                            PercentagePaid = expense.PercentagePaid,
                            Advance = expense.Advance,
                            Images = expense.Images,
                            Status = expense.Status,
                            ExpenseType = expenseType 
                        };

            int totalCount = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            return (items, totalCount);
        }
    }
}
