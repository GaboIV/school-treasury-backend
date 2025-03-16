using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IInterestLinkRepository
    {
        Task<IEnumerable<InterestLink>> GetAllAsync();
        Task<InterestLink> GetByIdAsync(string id);
        Task<InterestLink> CreateAsync(InterestLink interestLink);
        Task<InterestLink> UpdateAsync(string id, InterestLink interestLink);
        Task<bool> DeleteAsync(string id);
    }
} 