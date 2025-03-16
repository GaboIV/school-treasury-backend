using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class InterestLinkRepository : IInterestLinkRepository
    {
        private readonly IMongoCollection<InterestLink> _interestLinkCollection;

        public InterestLinkRepository(IMongoDatabase database)
        {
            _interestLinkCollection = database.GetCollection<InterestLink>("InterestLinks");
        }

        public async Task<IEnumerable<InterestLink>> GetAllAsync()
        {
            return await _interestLinkCollection.Find(link => link.Status == true)
                .SortBy(link => link.Order)
                .ToListAsync();
        }

        public async Task<InterestLink> GetByIdAsync(string id)
        {
            return await _interestLinkCollection.Find(link => link.Id == id && link.Status == true)
                .FirstOrDefaultAsync();
        }

        public async Task<InterestLink> CreateAsync(InterestLink interestLink)
        {
            interestLink.Status = true;
            await _interestLinkCollection.InsertOneAsync(interestLink);
            return interestLink;
        }

        public async Task<InterestLink> UpdateAsync(string id, InterestLink interestLink)
        {
            await _interestLinkCollection.ReplaceOneAsync(
                link => link.Id == id,
                interestLink);
            
            return interestLink;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var interestLink = await GetByIdAsync(id);
            if (interestLink == null)
            {
                return false;
            }

            interestLink.Status = false;
            interestLink.DeletedAt = DateTime.UtcNow;
            await _interestLinkCollection.ReplaceOneAsync(
                link => link.Id == id,
                interestLink);
            
            return true;
        }
    }
} 