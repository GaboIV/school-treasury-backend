using Domain.Entities;

namespace Application.Interfaces {
    public interface IExpenseTypeRepository
    {
        Task<List<ExpenseType>> GetAllAsync();
        Task<ExpenseType> GetByIdAsync(string id);
        Task InsertAsync(ExpenseType expenseType);
        Task UpdateAsync(ExpenseType expenseType);
        Task<bool> DeleteAsync(string id);
        Task<(List<ExpenseType> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
    }
}
