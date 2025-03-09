using Domain.Entities;

namespace Application.Interfaces {
    public interface IExpenseTypeRepository
    {
        Task<List<ExpenseType>> GetAllAsync();
        Task<ExpenseType> GetByIdAsync(string id);
        Task InsertAsync(ExpenseType expenseType);
        Task UpdateAsync(ExpenseType expenseType);
    }
}
