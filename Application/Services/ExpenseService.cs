using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services {
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;

        public ExpenseService(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public async Task<List<Expense>> GetAllExpensesAsync()
        {
            var expenses = await _expenseRepository.GetAllAsync();
            foreach(var expense in expenses)
            {
                Console.WriteLine("Estado: " + expense);
            }
            return expenses;
        }

        public async Task<Expense> GetExpenseByIdAsync(string id)
        {
            return await _expenseRepository.GetByIdAsync(id);
        }

        public async Task<Expense> CreateExpenseAsync(CreateExpenseDto dto)
        {
            decimal individualAmount = 0;
            int totalStudents = 25;
            
            if (dto.StudentQuantity == "all") {
                individualAmount = dto.TotalAmount / totalStudents;
            }

            var expense = new Expense
            {
                Name = dto.Name,
                ExpenseTypeId = dto.ExpenseTypeId!,
                Date = dto.Date,
                TotalAmount = dto.TotalAmount,
                IndividualAmount = individualAmount,
                Advance = new Advance(),
                StudentQuantity = dto.StudentQuantity
            };

            expense.Advance.Total = totalStudents;
            expense.Advance.Completed = 0;
            expense.Advance.Pending = totalStudents;

            await _expenseRepository.InsertAsync(expense);
            return expense;
        }

        public async Task<Expense> UpdateExpenseAsync(UpdateExpenseDto dto)
        {
            decimal individualAmount = 0;
            int totalStudents = 25;

            var existingExpense = await _expenseRepository.GetByIdAsync(dto.Id!);
            
            if (existingExpense == null)
                return null!;

            if (dto.StudentQuantity == "all") {
                individualAmount = dto.TotalAmount / totalStudents;
            }

            existingExpense.Name = dto.Name ?? "";
            existingExpense.ExpenseTypeId = dto.ExpenseTypeId ?? "";
            existingExpense.Date = dto.Date;
            existingExpense.TotalAmount = dto.TotalAmount;
            existingExpense.IndividualAmount = individualAmount;
            existingExpense.StudentQuantity = dto.StudentQuantity;
            existingExpense.Status = dto.Status;
            existingExpense.UpdatedAt = DateTime.UtcNow;

            existingExpense.Advance.Total = totalStudents;
            existingExpense.Advance.Pending = totalStudents - existingExpense.Advance.Completed;
            
            await _expenseRepository.UpdateAsync(existingExpense);
            return existingExpense;
        }

        public async Task<bool> DeleteExpenseAsync(string id)
        {
            var existingExpense = await _expenseRepository.GetByIdAsync(id);
            
            if (existingExpense == null)
                return false;

            // Verificar si existen gastos asociados a este tipo de gasto
            var existsExpenses = await ExistsExpenseWithTypeIdAsync(id);
            if (existsExpenses)
                return false;

            return await _expenseRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsExpenseWithTypeIdAsync(string expenseTypeId)
        {
            return await _expenseRepository.ExistsByExpenseTypeIdAsync(expenseTypeId);
        }

        public async Task<(List<Expense> Items, int TotalCount)> GetPaginatedExpensesAsync(int page, int pageSize)
        {
            return await _expenseRepository.GetPaginatedAsync(page, pageSize);
        }
    }
}

