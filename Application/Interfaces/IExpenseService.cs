using Application.DTOs;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces {
    public interface IExpenseService
    {
        Task<IEnumerable<Expense>> GetAllExpensesAsync();
        Task<Expense> GetExpenseByIdAsync(string id);
        Task<Expense> CreateExpenseAsync(CreateExpenseDto dto);
        Task<Expense?> UpdateExpenseAsync(UpdateExpenseDto dto);
        Task<bool> DeleteExpenseAsync(string id);
        Task<bool> ExistsExpenseWithTypeIdAsync(string expenseTypeId);
        Task<(IEnumerable<Expense> Expenses, int TotalCount)> GetPaginatedExpensesAsync(int page, int pageSize);
        Task<Expense> AdjustExpenseAmountAsync(string id, AdjustExpenseAmountDto dto);
    }
}

