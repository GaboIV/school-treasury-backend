using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services {
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expensiveRepository;

        public ExpenseService(IExpenseRepository expensiveRepository)
        {
            _expensiveRepository = expensiveRepository;
        }

        public async Task<List<Expense>> GetAllExpensesAsync()
        {
            return await _expensiveRepository.GetAllAsync();
        }

        public async Task<Expense> GetExpenseByIdAsync(string id)
        {
            return await _expensiveRepository.GetByIdAsync(id);
        }

        public async Task<Expense> CreateExpenseAsync(CreateExpenseDto dto)
        {
            var expensive = new Expense
            {
                Name = dto.Name,
                ExpenseTypeId = dto.ExpenseTypeId!,
                Date = dto.Date,
                TotalAmount = dto.TotalAmount,
                IndividualAmount = dto.IndividualAmount,
                Advance = new Advance()
            };

            expensive.Advance.Total = dto.Advance?.Total ?? 0;

            await _expensiveRepository.InsertAsync(expensive);
            return expensive;
        }
    }
}

