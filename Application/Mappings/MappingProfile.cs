using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Caja Chica
            CreateMap<PettyCash, PettyCashDto>();
            CreateMap<Transaction, TransactionDto>();
            CreateMap<CreateTransactionDto, Transaction>();
            
            // Logs de Transacciones
            CreateMap<TransactionLog, TransactionLogDto>();
        }
    }
} 