using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPettyCashRepository
    {
        Task<PettyCash> GetAsync();
        Task<PettyCash> CreateAsync(PettyCash pettyCash);
        Task<PettyCash> UpdateAsync(PettyCash pettyCash);
        Task<bool> ExistsAsync();
        Task UpdateBalanceAsync(decimal amount, TransactionType type);
    }
} 