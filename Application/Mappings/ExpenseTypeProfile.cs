using AutoMapper;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mapping
{
    public class ExpenseTypeProfile : Profile
    {
        public ExpenseTypeProfile()
        {
            CreateMap<ExpenseType, ExpenseTypeDto>();
        }
    }
}