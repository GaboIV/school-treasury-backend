using AutoMapper;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mapping
{
    public class CollectionTypeProfile : Profile
    {
        public CollectionTypeProfile()
        {
            CreateMap<CollectionType, CollectionTypeDto>();
        }
    }
}