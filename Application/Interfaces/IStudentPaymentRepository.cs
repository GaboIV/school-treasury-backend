using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStudentPaymentRepository
    {
        Task<IEnumerable<StudentPayment>> GetAllAsync();
        Task<StudentPayment?> GetByIdAsync(string id);
        Task<IEnumerable<StudentPayment>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<StudentPayment>> GetByCollectionIdAsync(string collectionId);
        Task<IEnumerable<StudentPayment>> GetPendingPaymentsByStudentIdAsync(string studentId);
        Task<StudentPayment> CreateAsync(StudentPayment payment);
        Task UpdateAsync(StudentPayment payment);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<StudentPayment>> CreateManyAsync(IEnumerable<StudentPayment> payments);
        Task UpdateManyAsync(IEnumerable<StudentPayment> payments);
        Task InsertAsync(StudentPayment payment);
        Task CreatePaymentsForCollectionAsync(string collectionId, decimal individualAmount);
        Task UpdatePaymentsForCollectionAsync(string collectionId, decimal newIndividualAmount);
    }
} 