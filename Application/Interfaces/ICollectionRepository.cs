using Domain.Entities;

namespace Application.Interfaces {
    public interface ICollectionRepository
    {
        Task<List<Collection>> GetAllAsync();
        Task<Collection> GetByIdAsync(string id);
        Task InsertAsync(Collection collection);
        Task UpdateAsync(Collection collection);
        Task<bool> ExistsByCollectionTypeIdAsync(string collectionId);
        Task<bool> DeleteAsync(string id);
        Task<(List<Collection> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
    }
}
