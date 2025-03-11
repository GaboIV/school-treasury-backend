using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStudentPaymentRepository
    {
        Task<IEnumerable<StudentPayment>> GetAllAsync();
        Task<StudentPayment> GetByIdAsync(string id);
        Task<IEnumerable<StudentPayment>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<StudentPayment>> GetByExpenseIdAsync(string expenseId);
        Task<IEnumerable<StudentPayment>> GetPendingPaymentsByStudentIdAsync(string studentId);
        Task<StudentPayment> CreateAsync(StudentPayment payment);
        Task UpdateAsync(StudentPayment payment);
        Task DeleteAsync(string id);
        Task<IEnumerable<StudentPayment>> CreateManyAsync(IEnumerable<StudentPayment> payments);
        Task UpdateManyAsync(IEnumerable<StudentPayment> payments);
    }
} 