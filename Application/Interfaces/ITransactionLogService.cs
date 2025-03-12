using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITransactionLogService
    {
        Task<IEnumerable<TransactionLogDto>> GetAllLogsAsync();
        Task<TransactionLogDto> GetLogByIdAsync(string id);
        Task<IEnumerable<TransactionLogDto>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TransactionLogDto>> GetLogsByRelatedEntityAsync(string relatedEntityId, string relatedEntityType);
        Task<TransactionLogDto> LogTransactionAsync(Transaction transaction, decimal balanceBefore, decimal balanceAfter, string userId = null, string userName = null, string ipAddress = null);
        Task<IEnumerable<TransactionTimelineDto>> GetTimelineAsync(int count = 20);
        Task<PaginatedResponseDto<IEnumerable<TransactionLogDto>>> GetPaginatedLogsAsync(int page, int pageSize);
    }
} 