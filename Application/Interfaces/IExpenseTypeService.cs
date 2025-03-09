using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces {
    public interface IExpenseTypeService
    {
        Task<List<ExpenseType>> GetAllExpenseTypesAsync();
        Task<ExpenseType> GetExpenseTypeByIdAsync(string id);
        Task<ExpenseType> CreateExpenseTypeAsync(CreateExpenseTypeDto dto);
        Task<ExpenseType> UpdateExpenseTypeAsync(UpdateExpenseTypeDto dto);
        Task<bool> DeleteExpenseTypeAsync(string id);
        Task<bool> ExistsExpenseWithTypeIdAsync(string expenseTypeId);
        Task<(List<ExpenseType> Items, int TotalCount)> GetPaginatedExpenseTypesAsync(int page, int pageSize);
    }
}

