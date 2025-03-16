using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class InterestLinkService : IInterestLinkService
    {
        private readonly IInterestLinkRepository _interestLinkRepository;
        private readonly IMapper _mapper;

        public InterestLinkService(IInterestLinkRepository interestLinkRepository, IMapper mapper)
        {
            _interestLinkRepository = interestLinkRepository ?? throw new ArgumentNullException(nameof(interestLinkRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<InterestLinkDto>> GetAllAsync()
        {
            var interestLinks = await _interestLinkRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<InterestLinkDto>>(interestLinks);
        }

        public async Task<InterestLinkDto> GetByIdAsync(string id)
        {
            var interestLink = await _interestLinkRepository.GetByIdAsync(id);
            return _mapper.Map<InterestLinkDto>(interestLink);
        }

        public async Task<InterestLinkDto> CreateAsync(CreateInterestLinkDto createInterestLinkDto)
        {
            var interestLink = _mapper.Map<InterestLink>(createInterestLinkDto);
            interestLink.CreatedAt = DateTime.UtcNow;
            interestLink.UpdatedAt = DateTime.UtcNow;

            var createdInterestLink = await _interestLinkRepository.CreateAsync(interestLink);
            return _mapper.Map<InterestLinkDto>(createdInterestLink);
        }

        public async Task<InterestLinkDto> UpdateAsync(string id, UpdateInterestLinkDto updateInterestLinkDto)
        {
            var existingInterestLink = await _interestLinkRepository.GetByIdAsync(id);
            if (existingInterestLink == null)
            {
                throw new KeyNotFoundException($"Interest link with id {id} not found");
            }

            _mapper.Map(updateInterestLinkDto, existingInterestLink);
            existingInterestLink.UpdatedAt = DateTime.UtcNow;

            var updatedInterestLink = await _interestLinkRepository.UpdateAsync(id, existingInterestLink);
            return _mapper.Map<InterestLinkDto>(updatedInterestLink);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _interestLinkRepository.DeleteAsync(id);
        }
    }
} 