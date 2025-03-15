using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class CollectionTypeRepository : ICollectionTypeRepository
    {
        private readonly IMongoCollection<CollectionType> _expensiveTypes;
        private readonly ILoggerManager _logger;

        public CollectionTypeRepository(IMongoDatabase database, ILoggerManager logger)
        {
            _expensiveTypes = database.GetCollection<CollectionType>("CollectionTypes");
            _logger = logger;
        }

        public async Task<List<CollectionType>> GetAllAsync()
        {
            _logger.LogDebug("Repositorio: Obteniendo todos los tipos de cobro");
            try
            {
                var result = await _expensiveTypes.Find(_ => true).ToListAsync();
                _logger.LogTrace($"Repositorio: Se obtuvieron {result.Count} tipos de cobro");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los tipos de cobro");
                throw;
            }
        }

        public async Task<CollectionType> GetByIdAsync(string id)
        {
            _logger.LogDebug($"Repositorio: Obteniendo tipo de cobro con ID: {id}");
            try
            {
                var result = await _expensiveTypes.Find(c => c.Id == id).FirstOrDefaultAsync();
                if (result == null)
                {
                    _logger.LogTrace($"Repositorio: No se encontró el tipo de cobro con ID: {id}");
                }
                else
                {
                    _logger.LogTrace($"Repositorio: Tipo de cobro encontrado con ID: {id}");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener tipo de cobro con ID: {id}");
                throw;
            }
        }

        public async Task InsertAsync(CollectionType expensiveType)
        {
            _logger.LogDebug($"Repositorio: Insertando nuevo tipo de cobro: {expensiveType.Name}");
            try
            {
                await _expensiveTypes.InsertOneAsync(expensiveType);
                _logger.LogTrace($"Repositorio: Tipo de cobro insertado con ID: {expensiveType.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al insertar tipo de cobro");
                throw;
            }
        }

        public async Task UpdateAsync(CollectionType expensiveType)
        {
            _logger.LogDebug($"Repositorio: Actualizando tipo de cobro con ID: {expensiveType.Id}");
            try
            {
                var filter = Builders<CollectionType>.Filter.Eq(c => c.Id, expensiveType.Id);
                
                // Utilizamos ReplaceOneAsync para reemplazar todo el documento
                // Esto evita tener que especificar cada campo a actualizar
                var result = await _expensiveTypes.ReplaceOneAsync(filter, expensiveType);
                
                if (result.ModifiedCount > 0)
                {
                    _logger.LogTrace($"Repositorio: Tipo de cobro actualizado con ID: {expensiveType.Id}");
                }
                else
                {
                    _logger.LogWarn($"Repositorio: No se actualizó ningún documento para el tipo de cobro con ID: {expensiveType.Id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar tipo de cobro con ID: {expensiveType.Id}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            _logger.LogDebug($"Repositorio: Eliminando tipo de cobro con ID: {id}");
            try
            {
                var result = await _expensiveTypes.DeleteOneAsync(c => c.Id == id);
                var success = result.DeletedCount > 0;
                
                if (success)
                {
                    _logger.LogTrace($"Repositorio: Tipo de cobro eliminado con ID: {id}");
                }
                else
                {
                    _logger.LogWarn($"Repositorio: No se encontró el tipo de cobro con ID: {id} para eliminar");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar tipo de cobro con ID: {id}");
                throw;
            }
        }

        public async Task<(List<CollectionType> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            _logger.LogDebug($"Repositorio: Obteniendo tipos de cobro paginados. Página: {page}, Tamaño: {pageSize}");
            try
            {
                var totalCount = await _expensiveTypes.CountDocumentsAsync(_ => true);
                
                var items = await _expensiveTypes.Find(_ => true)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();
                
                _logger.LogTrace($"Repositorio: Se obtuvieron {items.Count} tipos de cobro de un total de {totalCount}");
                return (items, (int)totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de cobro paginados");
                throw;
            }
        }
    }
}
