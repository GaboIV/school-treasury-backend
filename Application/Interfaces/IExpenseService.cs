using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces {
    public interface IExpenseService
    {
        Task<List<Expense>> GetAllExpensesAsync();
        Task<Expense> GetExpenseByIdAsync(string id);
        Task<Expense> CreateExpenseAsync(CreateExpenseDto dto);
    }
}

