using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class InterestLinkProfile : Profile
    {
        public InterestLinkProfile()
        {
            CreateMap<InterestLink, InterestLinkDto>();
            CreateMap<CreateInterestLinkDto, InterestLink>();
            CreateMap<UpdateInterestLinkDto, InterestLink>();
        }
    }
} 