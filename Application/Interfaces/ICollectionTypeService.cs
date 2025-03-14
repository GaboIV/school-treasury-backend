using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces {
    public interface ICollectionTypeService
    {
        Task<List<CollectionType>> GetAllCollectionTypesAsync();
        Task<CollectionType> GetCollectionTypeByIdAsync(string id);
        Task<CollectionType> CreateCollectionTypeAsync(CreateCollectionTypeDto dto);
        Task<CollectionType> UpdateCollectionTypeAsync(UpdateCollectionTypeDto dto);
        Task<bool> DeleteCollectionTypeAsync(string id);
        Task<bool> ExistsCollectionWithTypeIdAsync(string collectionTypeId);
        Task<(List<CollectionType> Items, int TotalCount)> GetPaginatedCollectionTypesAsync(int page, int pageSize);
    }
}

