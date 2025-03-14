using Application.DTOs;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStudentPaymentService
    {
        Task<IEnumerable<StudentPaymentDto>> GetAllPaymentsAsync();
        Task<StudentPaymentDto> GetPaymentByIdAsync(string id);
        Task<IEnumerable<StudentPaymentDto>> GetPaymentsByStudentIdAsync(string studentId);
        Task<IEnumerable<StudentPaymentDto>> GetPaymentsByCollectionIdAsync(string collectionId);
        Task<IEnumerable<StudentPaymentDto>> GetPendingPaymentsByStudentIdAsync(string studentId);
        Task<StudentPaymentDto> CreatePaymentAsync(CreateStudentPaymentDto dto);
        Task<StudentPaymentDto> UpdatePaymentAsync(string id, UpdateStudentPaymentDto dto);
        Task DeletePaymentAsync(string id);
        Task<IEnumerable<StudentPaymentDto>> CreatePaymentsForCollectionAsync(string collectionId, decimal individualAmount);
        Task UpdatePaymentsForCollectionAsync(string collectionId, decimal newIndividualAmount);
        Task<StudentPaymentDto> RegisterPaymentWithImagesAsync(RegisterPaymentWithImagesDto dto);
    }
} 