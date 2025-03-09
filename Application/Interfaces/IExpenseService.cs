using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces {
    public interface IExpenseService
    {
        Task<List<Expense>> GetAllExpensesAsync();
        Task<Expense> GetExpenseByIdAsync(string id);
        Task<Expense> CreateExpenseAsync(CreateExpenseDto dto);
        Task<Expense> UpdateExpenseAsync(UpdateExpenseDto dto);
        Task<bool> DeleteExpenseAsync(string id);
        Task<bool> ExistsExpenseWithTypeIdAsync(string expenseTypeId);
        Task<(List<Expense> Items, int TotalCount)> GetPaginatedExpensesAsync(int page, int pageSize);
    }
}

