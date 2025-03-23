using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPaymentRequestService
    {
        // Métodos básicos CRUD
        Task<IEnumerable<PaymentRequestDto>> GetAllPaymentRequestsAsync();
        Task<PaymentRequestDto> GetPaymentRequestByIdAsync(string id);
        Task<IEnumerable<PaymentRequestDto>> GetPaymentRequestsByStudentIdAsync(string studentId);
        Task<IEnumerable<PaymentRequestDto>> GetPaymentRequestsByStatusAsync(Domain.Entities.PaymentRequestStatus status);
        Task<PaymentRequestDto> CreatePaymentRequestAsync(CreatePaymentRequestDto dto, string userId);
        Task<PaymentRequestDto> CreatePaymentRequestWithImagesAsync(CreatePaymentRequestWithImagesDto dto, string userId);
        Task<PaymentRequestDto> UpdatePaymentRequestAsync(string id, UpdatePaymentRequestDto dto, string userId);
        Task<PaymentRequestDto> UpdatePaymentRequestWithImagesAsync(string id, UpdatePaymentRequestWithImagesDto dto, string userId);
        Task DeletePaymentRequestAsync(string id);
        
        // Métodos específicos para el flujo de trabajo de solicitudes
        Task<PaymentRequestDto> ApprovePaymentRequestAsync(string id, ApprovePaymentRequestDto dto);
        Task<PaymentRequestDto> RejectPaymentRequestAsync(string id, RejectPaymentRequestDto dto);
        Task<PaymentRequestDto> RequestChangesAsync(string id, RequestChangesDto dto);
        Task<PaymentRequestDto> AddAdminCommentAsync(string id, AddAdminCommentDto dto);
        Task<PaymentRequestDto> ChangeStatusAsync(string id, Domain.Entities.PaymentRequestStatus newStatus, string userId, string userRole, string details);
        
        // Métodos para obtener las solicitudes pendientes de revisión
        Task<IEnumerable<PaymentRequestDto>> GetPendingRequestsAsync();
        Task<IEnumerable<PaymentRequestDto>> GetUnderReviewRequestsAsync();
        Task<IEnumerable<PaymentRequestDto>> GetNeedsChangesRequestsAsync();
        
        // Métodos para obtener el historial de solicitudes por estudiante
        Task<IEnumerable<PaymentRequestDto>> GetRequestHistoryByStudentIdAsync(string studentId);
    }
} 