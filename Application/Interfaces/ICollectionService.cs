using Application.DTOs;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces {
    public interface ICollectionService
    {
        Task<IEnumerable<Collection>> GetAllCollectionsAsync();
        Task<Collection> GetCollectionByIdAsync(string id);
        Task<Collection> CreateCollectionAsync(CreateCollectionDto dto);
        Task<Collection?> UpdateCollectionAsync(UpdateCollectionDto dto);
        Task<bool> DeleteCollectionAsync(string id);
        Task<bool> ExistsCollectionWithTypeIdAsync(string collectionTypeId);
        Task<(IEnumerable<Collection> Collections, int TotalCount)> GetPaginatedCollectionsAsync(int page, int pageSize);
        Task<Collection> AdjustCollectionAmountAsync(string id, AdjustCollectionAmountDto dto);
    }
}

