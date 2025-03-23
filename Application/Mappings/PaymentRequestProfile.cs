using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class PaymentRequestProfile : Profile
    {
        public PaymentRequestProfile()
        {
            // Mapeo de PaymentRequest a PaymentRequestDto
            CreateMap<PaymentRequest, PaymentRequestDto>()
                .ForMember(dest => dest.HistoryEntries, opt => opt.MapFrom(src => src.HistoryEntries))
                .ForMember(dest => dest.AdminComments, opt => opt.MapFrom(src => src.AdminComments));
                
            // Mapeo de PaymentRequestHistoryEntry a PaymentRequestHistoryEntryDto
            CreateMap<PaymentRequestHistoryEntry, PaymentRequestHistoryEntryDto>()
                .ForMember(dest => dest.UserName, opt => opt.Ignore()); // Se establecerá en el servicio
                
            // Mapeo de AdminComment a AdminCommentDto
            CreateMap<AdminComment, AdminCommentDto>();
            
            // Mapeos inversos (de DTOs a entidades)
            CreateMap<CreatePaymentRequestDto, PaymentRequest>();
            CreateMap<UpdatePaymentRequestDto, PaymentRequest>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
} 