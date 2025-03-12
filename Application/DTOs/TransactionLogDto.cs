using Domain.Entities;
using System;

namespace Application.DTOs
{
    public class TransactionLogDto : BaseDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
    }

    public class TransactionTimelineDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
    }
} 