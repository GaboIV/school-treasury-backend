using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITransactionLogRepository
    {
        Task<IEnumerable<TransactionLog>> GetAllAsync();
        Task<TransactionLog> GetByIdAsync(string id);
        Task<IEnumerable<TransactionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TransactionLog>> GetByRelatedEntityAsync(string relatedEntityId, string relatedEntityType);
        Task<TransactionLog> AddAsync(TransactionLog log);
        Task<(IEnumerable<TransactionLog> Logs, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
    }
} 