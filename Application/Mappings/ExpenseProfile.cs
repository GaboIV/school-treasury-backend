using AutoMapper;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mapping
{
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            CreateMap<Expense, ExpenseDto>();
            CreateMap<Advance, TotalPaidDto>();
            CreateMap<Image, ImageDto>();
        }
    }
}