using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetAllAsync();
        Task<Transaction> GetByIdAsync(string id);
        Task<Transaction> CreateAsync(Transaction transaction);
        Task<List<Transaction>> GetPaginatedAsync(int page, int pageSize);
        Task<List<Transaction>> GetByRelatedEntityAsync(string relatedEntityId, string relatedEntityType);
        Task<int> GetTotalCountAsync();
    }
} 