using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IInterestLinkService
    {
        Task<IEnumerable<InterestLinkDto>> GetAllAsync();
        Task<InterestLinkDto> GetByIdAsync(string id);
        Task<InterestLinkDto> CreateAsync(CreateInterestLinkDto createInterestLinkDto);
        Task<InterestLinkDto> UpdateAsync(string id, UpdateInterestLinkDto updateInterestLinkDto);
        Task<bool> DeleteAsync(string id);
    }
} 