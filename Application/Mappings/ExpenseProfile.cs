using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            // Mapeos para Expense
            CreateMap<Expense, ExpenseDto>();
            CreateMap<CreateExpenseDto, Expense>();
            CreateMap<UpdateExpenseDto, Expense>();
            CreateMap<Image, ImageDto>();
        }
    }
} 