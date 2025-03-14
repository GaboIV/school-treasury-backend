using AutoMapper;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mapping
{
    public class CollectionProfile : Profile
    {
        public CollectionProfile()
        {
            CreateMap<Collection, CollectionDto>();
            CreateMap<Advance, TotalPaidDto>();
            CreateMap<Image, ImageDto>();
        }
    }
}