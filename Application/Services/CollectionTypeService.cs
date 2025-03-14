using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services {
    public class CollectionTypeService : ICollectionTypeService
    {
        private readonly ICollectionTypeRepository _expensiveTypeRepository;
        private readonly ICollectionRepository _collectionRepository;

        public CollectionTypeService(ICollectionTypeRepository expensiveTypeRepository, ICollectionRepository collectionRepository)
        {
            _expensiveTypeRepository = expensiveTypeRepository;
            _collectionRepository = collectionRepository;
        }

        public async Task<List<CollectionType>> GetAllCollectionTypesAsync()
        {
            return await _expensiveTypeRepository.GetAllAsync();
        }

        public async Task<CollectionType> GetCollectionTypeByIdAsync(string id)
        {
            return await _expensiveTypeRepository.GetByIdAsync(id);
        }

        public async Task<CollectionType> CreateCollectionTypeAsync(CreateCollectionTypeDto dto)
        {
            var expensiveType = new CollectionType
            {
                Name = dto.Name ?? ""
            };

            await _expensiveTypeRepository.InsertAsync(expensiveType);
            return expensiveType;
        }

        public async Task<CollectionType> UpdateCollectionTypeAsync(UpdateCollectionTypeDto dto)
        {
            var existingCollectionType = await _expensiveTypeRepository.GetByIdAsync(dto.Id!);
            
            if (existingCollectionType == null)
                return null!;

            existingCollectionType.Name = dto.Name ?? "";
            existingCollectionType.UpdatedAt = DateTime.UtcNow;
            
            await _expensiveTypeRepository.UpdateAsync(existingCollectionType);
            return existingCollectionType;
        }

        public async Task<bool> DeleteCollectionTypeAsync(string id)
        {
            var existingCollectionType = await _expensiveTypeRepository.GetByIdAsync(id);
            
            if (existingCollectionType == null)
                return false;

            // Verificar si existen gastos asociados a este tipo de gasto
            var existsCollections = await ExistsCollectionWithTypeIdAsync(id);
            if (existsCollections)
                return false;

            return await _expensiveTypeRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsCollectionWithTypeIdAsync(string collectionTypeId)
        {
            return await _collectionRepository.ExistsByCollectionTypeIdAsync(collectionTypeId);
        }

        public async Task<(List<CollectionType> Items, int TotalCount)> GetPaginatedCollectionTypesAsync(int page, int pageSize)
        {
            return await _expensiveTypeRepository.GetPaginatedAsync(page, pageSize);
        }
    }
}

