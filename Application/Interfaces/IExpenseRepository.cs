using Domain.Entities;

namespace Application.Interfaces {
    public interface IExpenseRepository
    {
        Task<List<Expense>> GetAllAsync();
        Task<Expense> GetByIdAsync(string id);
        Task InsertAsync(Expense expense);
        Task UpdateAsync(Expense expense);
    }
}
