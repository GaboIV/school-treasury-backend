using Domain.Entities;

namespace Application.Interfaces {
    public interface ICollectionTypeRepository
    {
        Task<List<CollectionType>> GetAllAsync();
        Task<CollectionType> GetByIdAsync(string id);
        Task InsertAsync(CollectionType collectionType);
        Task UpdateAsync(CollectionType collectionType);
        Task<bool> DeleteAsync(string id);
        Task<(List<CollectionType> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
    }
}
