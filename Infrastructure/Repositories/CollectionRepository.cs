using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Infrastructure.Repositories
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly IMongoCollection<Collection> _collections;
        private readonly IMongoCollection<CollectionType> _collectionTypes;

        public CollectionRepository(MongoDbContext context)
        {
            _collections = context.Collections;
            _collectionTypes = context.CollectionTypes;
        }

        public async Task<List<Collection>> GetAllAsync() =>
            await _collections.Find(_ => true).ToListAsync();

        public async Task<Collection> GetByIdAsync(string id) =>
            await _collections.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task InsertAsync(Collection expensive) =>
            await _collections.InsertOneAsync(expensive);

        public async Task UpdateAsync(Collection expensive)
        {
            var filter = Builders<Collection>.Filter.Eq(c => c.Id, expensive.Id);
            
            // Utilizamos ReplaceOneAsync para reemplazar todo el documento
            // Esto evita tener que especificar cada campo a actualizar
            await _collections.ReplaceOneAsync(filter, expensive);
        }

        public async Task<bool> ExistsByCollectionTypeIdAsync(string collectionTypeId)
        {
            var count = await _collections.CountDocumentsAsync(c => c.CollectionTypeId == collectionTypeId);
            return count > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collections.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<(List<Collection> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            var query = from collection in _collections.AsQueryable()
                        join collectionType in _collectionTypes.AsQueryable()
                        on collection.CollectionTypeId equals collectionType.Id into collectionGroup
                        from collectionType in collectionGroup.DefaultIfEmpty()
                        select new Collection
                        {
                            Id = collection.Id,
                            CollectionTypeId = collection.CollectionTypeId,
                            Name = collection.Name,
                            TotalAmount = collection.TotalAmount,
                            IndividualAmount = collection.IndividualAmount,
                            AdjustedIndividualAmount = collection.AdjustedIndividualAmount,
                            TotalSurplus = collection.TotalSurplus,
                            Date = collection.Date,
                            PercentagePaid = collection.PercentagePaid,
                            Advance = collection.Advance,
                            Images = collection.Images,
                            Status = collection.Status,
                            CollectionType = collectionType,
                            AllowsExemptions = collection.AllowsExemptions
                        };

            int totalCount = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            return (items, totalCount);
        }
    }
}
