using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services {
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IStudentPaymentRepository _studentPaymentRepository;

        public ExpenseService(
            IExpenseRepository expenseRepository,
            IStudentPaymentRepository studentPaymentRepository)
        {
            _expenseRepository = expenseRepository;
            _studentPaymentRepository = studentPaymentRepository;
        }

        public async Task<IEnumerable<Expense>> GetAllExpensesAsync()
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
            
            // Crear pagos para cada estudiante
            if (dto.StudentQuantity == "all")
            {
                await _studentPaymentRepository.CreatePaymentsForExpenseAsync(expense.Id, individualAmount);
            }
            
            return expense;
        }

        public async Task<Expense?> UpdateExpenseAsync(UpdateExpenseDto dto)
        {
            decimal individualAmount = 0;
            int totalStudents = 25;

            var existingExpense = await _expenseRepository.GetByIdAsync(dto.Id!);
            
            if (existingExpense == null)
                return null;

            if (dto.StudentQuantity == "all") {
                individualAmount = dto.TotalAmount / totalStudents;
            }

            // Guardar el monto individual anterior para comparar
            decimal previousIndividualAmount = existingExpense.IndividualAmount;
            string previousStudentQuantity = existingExpense.StudentQuantity;

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
            
            // Actualizar pagos de estudiantes si el monto individual cambió
            if (dto.StudentQuantity == "all")
            {
                if (previousStudentQuantity != "all")
                {
                    // Si antes no era para todos los estudiantes, crear los pagos
                    await _studentPaymentRepository.CreatePaymentsForExpenseAsync(existingExpense.Id, individualAmount);
                }
                else if (previousIndividualAmount != individualAmount)
                {
                    // Si el monto individual cambió, actualizar los pagos existentes
                    await _studentPaymentRepository.UpdatePaymentsForExpenseAsync(existingExpense.Id, individualAmount);
                }
            }
            
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

        public async Task<(IEnumerable<Expense> Expenses, int TotalCount)> GetPaginatedExpensesAsync(int page, int pageSize)
        {
            var result = await _expenseRepository.GetPaginatedAsync(page, pageSize);
            return (result.Items, result.TotalCount);
        }

        public async Task<Expense> AdjustExpenseAmountAsync(string id, AdjustExpenseAmountDto dto)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null)
            {
                throw new KeyNotFoundException($"Gasto con ID {id} no encontrado");
            }

            // Actualizar el monto ajustado y el excedente total
            expense.AdjustedIndividualAmount = dto.AdjustedAmount;
            expense.TotalSurplus = dto.Surplus;

            // Obtener todos los pagos relacionados con este gasto
            var payments = await _studentPaymentRepository.GetByExpenseIdAsync(id);

            // Actualizar cada pago
            foreach (var payment in payments)
            {
                // Mantener el monto original
                payment.AmountExpense = expense.IndividualAmount;
                
                // Establecer el monto ajustado
                payment.AdjustedAmountExpense = dto.AdjustedAmount;
                
                // Calcular el excedente individual
                payment.Surplus = dto.Surplus;

                // Recalcular el estado del pago
                if (payment.AmountPaid >= payment.AdjustedAmountExpense)
                {
                    payment.PaymentStatus = PaymentStatus.Paid;
                    payment.Excedent = payment.AmountPaid - payment.AdjustedAmountExpense;
                    payment.Pending = 0;
                }
                else if (payment.AmountPaid > 0)
                {
                    payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                    payment.Excedent = 0;
                    payment.Pending = payment.AdjustedAmountExpense - payment.AmountPaid;
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Pending;
                    payment.Excedent = 0;
                    payment.Pending = payment.AdjustedAmountExpense;
                }

                // Actualizar el pago en la base de datos
                await _studentPaymentRepository.UpdateAsync(payment);
            }

            // Actualizar el avance del gasto
            expense.Advance.Total = payments.Count();
            expense.Advance.Completed = payments.Count(p => p.PaymentStatus == PaymentStatus.Paid);
            expense.Advance.Pending = expense.Advance.Total - expense.Advance.Completed;

            // Calcular el porcentaje pagado
            var totalPaid = payments.Sum(p => p.AmountPaid);
            var totalAdjusted = (expense.AdjustedIndividualAmount ?? expense.IndividualAmount) * expense.Advance.Total;
            expense.PercentagePaid = totalAdjusted > 0 ? (totalPaid / totalAdjusted) * 100 : 0;

            // Guardar los cambios en el gasto
            await _expenseRepository.UpdateAsync(expense);

            return expense;
        }
    }
}

