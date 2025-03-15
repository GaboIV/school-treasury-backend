using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services {
    public class CollectionTypeService : ICollectionTypeService
    {
        private readonly ICollectionTypeRepository _expensiveTypeRepository;
        private readonly ICollectionRepository _collectionRepository;
        private readonly ILoggerManager _logger;

        public CollectionTypeService(
            ICollectionTypeRepository expensiveTypeRepository, 
            ICollectionRepository collectionRepository,
            ILoggerManager logger)
        {
            _expensiveTypeRepository = expensiveTypeRepository;
            _collectionRepository = collectionRepository;
            _logger = logger;
        }

        public async Task<List<CollectionType>> GetAllCollectionTypesAsync()
        {
            _logger.LogInfo("Obteniendo todos los tipos de cobro");
            var result = await _expensiveTypeRepository.GetAllAsync();
            _logger.LogInfo($"Se obtuvieron {result.Count} tipos de cobro");
            return result;
        }

        public async Task<CollectionType> GetCollectionTypeByIdAsync(string id)
        {
            _logger.LogInfo($"Obteniendo tipo de cobro con ID: {id}");
            var result = await _expensiveTypeRepository.GetByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarn($"No se encontró el tipo de cobro con ID: {id}");
            }
            else
            {
                _logger.LogInfo($"Tipo de cobro encontrado: {result.Name}");
            }
            return result;
        }

        public async Task<CollectionType> CreateCollectionTypeAsync(CreateCollectionTypeDto dto)
        {
            _logger.LogInfo($"Creando nuevo tipo de cobro: {dto.Name}");
            
            var expensiveType = new CollectionType
            {
                Name = dto.Name ?? ""
            };

            await _expensiveTypeRepository.InsertAsync(expensiveType);
            _logger.LogInfo($"Tipo de cobro creado con ID: {expensiveType.Id}");
            return expensiveType;
        }

        public async Task<CollectionType> UpdateCollectionTypeAsync(UpdateCollectionTypeDto dto)
        {
            _logger.LogInfo($"Actualizando tipo de cobro con ID: {dto.Id}");
            
            var existingCollectionType = await _expensiveTypeRepository.GetByIdAsync(dto.Id!);
            
            if (existingCollectionType == null)
            {
                _logger.LogWarn($"No se encontró el tipo de cobro con ID: {dto.Id} para actualizar");
                return null!;
            }

            _logger.LogDebug($"Tipo de cobro encontrado. Nombre anterior: {existingCollectionType.Name}, Nuevo nombre: {dto.Name}");
            
            existingCollectionType.Name = dto.Name ?? "";
            existingCollectionType.UpdatedAt = DateTime.UtcNow;
            
            await _expensiveTypeRepository.UpdateAsync(existingCollectionType);
            _logger.LogInfo($"Tipo de cobro actualizado correctamente con ID: {existingCollectionType.Id}");
            return existingCollectionType;
        }

        public async Task<bool> DeleteCollectionTypeAsync(string id)
        {
            _logger.LogInfo($"Eliminando tipo de cobro con ID: {id}");
            
            var existingCollectionType = await _expensiveTypeRepository.GetByIdAsync(id);
            
            if (existingCollectionType == null)
            {
                _logger.LogWarn($"No se encontró el tipo de cobro con ID: {id} para eliminar");
                return false;
            }

            // Verificar si existen cobros asociados a este tipo de cobro
            var existsCollections = await ExistsCollectionWithTypeIdAsync(id);
            if (existsCollections)
            {
                _logger.LogWarn($"No se puede eliminar el tipo de cobro con ID: {id} porque tiene cobros asociados");
                return false;
            }

            var result = await _expensiveTypeRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInfo($"Tipo de cobro eliminado correctamente con ID: {id}");
            }
            else
            {
                _logger.LogError($"Error al eliminar el tipo de cobro con ID: {id}");
            }
            return result;
        }

        public async Task<bool> ExistsCollectionWithTypeIdAsync(string collectionTypeId)
        {
            _logger.LogDebug($"Verificando si existen cobros asociados al tipo de cobro con ID: {collectionTypeId}");
            var result = await _collectionRepository.ExistsByCollectionTypeIdAsync(collectionTypeId);
            _logger.LogDebug($"Resultado de verificación para tipo de cobro con ID {collectionTypeId}: {(result ? "Tiene cobros asociados" : "No tiene cobros asociados")}");
            return result;
        }

        public async Task<(List<CollectionType> Items, int TotalCount)> GetPaginatedCollectionTypesAsync(int page, int pageSize)
        {
            _logger.LogInfo($"Obteniendo tipos de cobro paginados. Página: {page}, Tamaño de página: {pageSize}");
            var result = await _expensiveTypeRepository.GetPaginatedAsync(page, pageSize);
            _logger.LogInfo($"Se obtuvieron {result.Items.Count} tipos de cobro de un total de {result.TotalCount}");
            return result;
        }
    }
}

