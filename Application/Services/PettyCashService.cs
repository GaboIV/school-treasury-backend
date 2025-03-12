using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PettyCashService : IPettyCashService
    {
        private readonly IPettyCashRepository _pettyCashRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IStudentPaymentRepository _studentPaymentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PettyCashService> _logger;

        public PettyCashService(
            IPettyCashRepository pettyCashRepository,
            ITransactionRepository transactionRepository,
            IStudentPaymentRepository studentPaymentRepository,
            IStudentRepository studentRepository,
            IExpenseRepository expenseRepository,
            IMapper mapper,
            ILogger<PettyCashService> logger)
        {
            _pettyCashRepository = pettyCashRepository;
            _transactionRepository = transactionRepository;
            _studentPaymentRepository = studentPaymentRepository;
            _studentRepository = studentRepository;
            _expenseRepository = expenseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PettyCashDto> GetPettyCashAsync()
        {
            var pettyCash = await _pettyCashRepository.GetAsync();
            if (pettyCash == null)
            {
                pettyCash = new PettyCash();
                await _pettyCashRepository.CreateAsync(pettyCash);
            }

            return _mapper.Map<PettyCashDto>(pettyCash);
        }

        public async Task<TransactionSummaryDto> GetSummaryAsync()
        {
            var pettyCash = await _pettyCashRepository.GetAsync();
            if (pettyCash == null)
            {
                pettyCash = new PettyCash();
                await _pettyCashRepository.CreateAsync(pettyCash);
            }

            var recentTransactions = await _transactionRepository.GetPaginatedAsync(1, 5);

            return new TransactionSummaryDto
            {
                Balance = pettyCash.CurrentBalance,
                TotalIncome = pettyCash.TotalIncome,
                TotalExpense = pettyCash.TotalExpense,
                LastTransactionDate = recentTransactions.Any() ? recentTransactions.First().Date : (DateTime?)null,
                RecentTransactions = _mapper.Map<List<TransactionDto>>(recentTransactions)
            };
        }

        public async Task<TransactionDto> AddTransactionAsync(CreateTransactionDto transactionDto)
        {
            var transaction = _mapper.Map<Transaction>(transactionDto);
            
            // Actualizar el balance en la caja chica
            await _pettyCashRepository.UpdateBalanceAsync(transaction.Amount, transaction.Type);
            
            // Guardar la transacción en la colección separada
            var addedTransaction = await _transactionRepository.CreateAsync(transaction);
            
            return _mapper.Map<TransactionDto>(addedTransaction);
        }

        public async Task<TransactionDto> RegisterExpenseFromPaymentAsync(string entityId)
        {
            try
            {
                // Intentar obtener el pago
                var payment = await _studentPaymentRepository.GetByIdAsync(entityId);
                
                // Si no es un pago, intentar obtener un gasto
                if (payment == null)
                {
                    var expenseEntity = await _expenseRepository.GetByIdAsync(entityId);
                    if (expenseEntity == null)
                    {
                        throw new KeyNotFoundException($"No se encontró ninguna entidad con el ID {entityId}");
                    }
                    
                    // Es un gasto
                    return await RegisterExpenseFromPaymentAsync(
                        entityId,
                        expenseEntity.TotalAmount,
                        $"Gasto: {expenseEntity.Name}"
                    );
                }
                
                // Obtener información del estudiante
                var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                
                // Obtener información del gasto
                var expense = await _expenseRepository.GetByIdAsync(payment.ExpenseId);
                
                // Es un pago
                return await RegisterExpenseFromPaymentAsync(
                    entityId,
                    payment.AmountPaid,
                    $"Pago de estudiante: {student?.Name ?? "Desconocido"} - Gasto: {expense?.Name ?? "Desconocido"}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar gasto desde el pago {entityId}");
                throw;
            }
        }

        public async Task<TransactionDto> RegisterExpenseFromPaymentAsync(string entityId, decimal amount, string description)
        {
            try
            {
                // Obtener información adicional si es un pago
                string? studentId = null;
                string? studentName = null;
                string? expenseId = null;
                string? expenseName = null;
                string? paymentId = null;
                string? paymentStatus = null;
                
                var payment = await _studentPaymentRepository.GetByIdAsync(entityId);
                if (payment != null)
                {
                    paymentId = payment.Id;
                    paymentStatus = payment.PaymentStatus.ToString();
                    studentId = payment.StudentId;
                    
                    // Obtener información del estudiante
                    var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                    studentName = student?.Name;
                    
                    // Obtener información del gasto
                    expenseId = payment.ExpenseId;
                    var expense = await _expenseRepository.GetByIdAsync(payment.ExpenseId);
                    expenseName = expense?.Name;
                }
                
                var transaction = new Transaction
                {
                    Type = TransactionType.Expense,
                    Amount = amount,
                    Description = description,
                    RelatedEntityId = entityId,
                    RelatedEntityType = payment != null ? "Payment" : "Expense",
                    StudentId = studentId,
                    StudentName = studentName,
                    ExpenseId = expenseId,
                    ExpenseName = expenseName,
                    PaymentId = paymentId,
                    PaymentStatus = paymentStatus
                };

                // Actualizar el balance en la caja chica
                await _pettyCashRepository.UpdateBalanceAsync(amount, TransactionType.Expense);
                
                // Guardar la transacción en la colección separada
                var addedTransaction = await _transactionRepository.CreateAsync(transaction);
                
                return _mapper.Map<TransactionDto>(addedTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar gasto desde la entidad {entityId}");
                throw;
            }
        }

        public async Task<TransactionDto> RegisterIncomeFromExcedentAsync(string paymentId, decimal amount, string description)
        {
            try
            {
                // Obtener información del pago
                var payment = await _studentPaymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    throw new KeyNotFoundException($"No se encontró el pago con ID {paymentId}");
                }
                
                // Obtener información del estudiante
                var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                
                // Obtener información del gasto
                var expense = await _expenseRepository.GetByIdAsync(payment.ExpenseId);
                
                var transaction = new Transaction
                {
                    Type = TransactionType.Income,
                    Amount = amount,
                    Description = description,
                    RelatedEntityId = paymentId,
                    RelatedEntityType = "Payment",
                    StudentId = payment.StudentId,
                    StudentName = student?.Name,
                    ExpenseId = payment.ExpenseId,
                    ExpenseName = expense?.Name,
                    PaymentId = payment.Id,
                    PaymentStatus = payment.PaymentStatus.ToString()
                };

                // Actualizar el balance en la caja chica
                await _pettyCashRepository.UpdateBalanceAsync(amount, TransactionType.Income);
                
                // Guardar la transacción en la colección separada
                var addedTransaction = await _transactionRepository.CreateAsync(transaction);
                
                return _mapper.Map<TransactionDto>(addedTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar ingreso desde el excedente {paymentId}");
                throw;
            }
        }

        public async Task<PaginatedTransactionDto> GetTransactionsAsync(int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                // Obtener las transacciones paginadas desde el repositorio de transacciones
                var transactions = await _transactionRepository.GetPaginatedAsync(pageIndex + 1, pageSize);
                var totalCount = await _transactionRepository.GetTotalCountAsync();
                
                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);
                
                return new PaginatedTransactionDto
                {
                    Items = transactionDtos,
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones paginadas");
                throw;
            }
        }
    }
} 