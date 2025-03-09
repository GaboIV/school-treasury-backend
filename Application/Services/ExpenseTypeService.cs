using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services {
    public class ExpenseTypeService : IExpenseTypeService
    {
        private readonly IExpenseTypeRepository _expensiveTypeRepository;
        private readonly IExpenseRepository _expenseRepository;

        public ExpenseTypeService(IExpenseTypeRepository expensiveTypeRepository, IExpenseRepository expenseRepository)
        {
            _expensiveTypeRepository = expensiveTypeRepository;
            _expenseRepository = expenseRepository;
        }

        public async Task<List<ExpenseType>> GetAllExpenseTypesAsync()
        {
            return await _expensiveTypeRepository.GetAllAsync();
        }

        public async Task<ExpenseType> GetExpenseTypeByIdAsync(string id)
        {
            return await _expensiveTypeRepository.GetByIdAsync(id);
        }

        public async Task<ExpenseType> CreateExpenseTypeAsync(CreateExpenseTypeDto dto)
        {
            var expensiveType = new ExpenseType
            {
                Name = dto.Name ?? ""
            };

            await _expensiveTypeRepository.InsertAsync(expensiveType);
            return expensiveType;
        }

        public async Task<ExpenseType> UpdateExpenseTypeAsync(UpdateExpenseTypeDto dto)
        {
            var existingExpenseType = await _expensiveTypeRepository.GetByIdAsync(dto.Id!);
            
            if (existingExpenseType == null)
                return null!;

            existingExpenseType.Name = dto.Name ?? "";
            existingExpenseType.UpdatedAt = DateTime.UtcNow;
            
            await _expensiveTypeRepository.UpdateAsync(existingExpenseType);
            return existingExpenseType;
        }

        public async Task<bool> DeleteExpenseTypeAsync(string id)
        {
            var existingExpenseType = await _expensiveTypeRepository.GetByIdAsync(id);
            
            if (existingExpenseType == null)
                return false;

            // Verificar si existen gastos asociados a este tipo de gasto
            var existsExpenses = await ExistsExpenseWithTypeIdAsync(id);
            if (existsExpenses)
                return false;

            return await _expensiveTypeRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsExpenseWithTypeIdAsync(string expenseTypeId)
        {
            return await _expenseRepository.ExistsByExpenseTypeIdAsync(expenseTypeId);
        }

        public async Task<(List<ExpenseType> Items, int TotalCount)> GetPaginatedExpenseTypesAsync(int page, int pageSize)
        {
            return await _expensiveTypeRepository.GetPaginatedAsync(page, pageSize);
        }
    }
}

