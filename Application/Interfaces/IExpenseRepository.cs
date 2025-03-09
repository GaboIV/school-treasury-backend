using Domain.Entities;

namespace Application.Interfaces {
    public interface IExpenseRepository
    {
        Task<List<Expense>> GetAllAsync();
        Task<Expense> GetByIdAsync(string id);
        Task InsertAsync(Expense expense);
        Task UpdateAsync(Expense expense);
        Task<bool> ExistsByExpenseTypeIdAsync(string expenseId);
        Task<bool> DeleteAsync(string id);
        Task<(List<Expense> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
    }
}
