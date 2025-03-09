using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services {
    public class ExpenseTypeService : IExpenseTypeService
    {
        private readonly IExpenseTypeRepository _expensiveTypeRepository;

        public ExpenseTypeService(IExpenseTypeRepository expensiveTypeRepository)
        {
            _expensiveTypeRepository = expensiveTypeRepository;
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
    }
}

