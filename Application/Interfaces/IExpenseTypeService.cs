using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces {
    public interface IExpenseTypeService
    {
        Task<List<ExpenseType>> GetAllExpenseTypesAsync();
        Task<ExpenseType> GetExpenseTypeByIdAsync(string id);
        Task<ExpenseType> CreateExpenseTypeAsync(CreateExpenseTypeDto dto);
    }
}

