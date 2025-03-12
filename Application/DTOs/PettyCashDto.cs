using Domain.Entities;
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class PettyCashDto : BaseDto
    {
        public decimal Balance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class TransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public DateTime Date { get; set; }
        public string? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        
        // Información adicional del estudiante
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        
        // Información adicional del pago
        public string? ExpenseId { get; set; }
        public string? ExpenseName { get; set; }
        public string? PaymentId { get; set; }
        public string? PaymentStatus { get; set; }
        
        // Información adicional para el seguimiento
        public string? CreatedBy { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateTransactionDto
    {
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        
        // Información adicional del estudiante
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        
        // Información adicional del pago
        public string? ExpenseId { get; set; }
        public string? ExpenseName { get; set; }
        public string? PaymentId { get; set; }
        public string? PaymentStatus { get; set; }
        
        // Información adicional para el seguimiento
        public string? CreatedBy { get; set; }
        public string? Notes { get; set; }
    }

    public class TransactionSummaryDto
    {
        public decimal Balance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public List<TransactionDto> RecentTransactions { get; set; } = new List<TransactionDto>();
    }
} 