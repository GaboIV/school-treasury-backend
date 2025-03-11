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
        Task<IEnumerable<StudentPaymentDto>> GetPaymentsByExpenseIdAsync(string expenseId);
        Task<IEnumerable<StudentPaymentDto>> GetPendingPaymentsByStudentIdAsync(string studentId);
        Task<StudentPaymentDto> CreatePaymentAsync(CreateStudentPaymentDto dto);
        Task<StudentPaymentDto> UpdatePaymentAsync(string id, UpdateStudentPaymentDto dto);
        Task DeletePaymentAsync(string id);
        Task<IEnumerable<StudentPaymentDto>> CreatePaymentsForExpenseAsync(string expenseId, decimal individualAmount);
        Task UpdatePaymentsForExpenseAsync(string expenseId, decimal newIndividualAmount);
        Task<StudentPaymentDto> RegisterPaymentWithImagesAsync(RegisterPaymentWithImagesDto dto);
    }
} 