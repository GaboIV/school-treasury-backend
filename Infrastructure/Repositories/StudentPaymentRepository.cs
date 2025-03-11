using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class StudentPaymentRepository : IStudentPaymentRepository
    {
        private readonly IMongoCollection<StudentPayment> _paymentCollection;

        public StudentPaymentRepository(IMongoDatabase database)
        {
            _paymentCollection = database.GetCollection<StudentPayment>("StudentPayments");
        }

        public async Task<IEnumerable<StudentPayment>> GetAllAsync()
        {
            return await _paymentCollection.Find(payment => payment.Status == true).ToListAsync();
        }

        public async Task<StudentPayment> GetByIdAsync(string id)
        {
            return await _paymentCollection.Find(payment => payment.Id == id && payment.Status == true).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<StudentPayment>> GetByStudentIdAsync(string studentId)
        {
            return await _paymentCollection.Find(payment => payment.StudentId == studentId && payment.Status == true).ToListAsync();
        }

        public async Task<IEnumerable<StudentPayment>> GetByExpenseIdAsync(string expenseId)
        {
            return await _paymentCollection.Find(payment => payment.ExpenseId == expenseId && payment.Status == true).ToListAsync();
        }

        public async Task<IEnumerable<StudentPayment>> GetPendingPaymentsByStudentIdAsync(string studentId)
        {
            return await _paymentCollection.Find(
                payment => payment.StudentId == studentId && 
                payment.Status == true && 
                (payment.PaymentStatus == PaymentStatus.Pending || payment.PaymentStatus == PaymentStatus.PartiallyPaid)
            ).ToListAsync();
        }

        public async Task<StudentPayment> CreateAsync(StudentPayment payment)
        {
            payment.Status = true;
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.Pending = payment.AmountExpense - payment.AmountPaid;
            
            await _paymentCollection.InsertOneAsync(payment);
            return payment;
        }

        public async Task UpdateAsync(StudentPayment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            payment.Pending = payment.AmountExpense - payment.AmountPaid;
            
            // Actualizar el estado del pago basado en los montos
            if (payment.AmountPaid >= payment.AmountExpense)
            {
                if (payment.AmountPaid > payment.AmountExpense)
                {
                    payment.Excedent = payment.AmountPaid - payment.AmountExpense;
                    payment.PaymentStatus = PaymentStatus.Excedent;
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Paid;
                }
                
                payment.Pending = 0;
                
                if (payment.PaymentDate == null)
                {
                    payment.PaymentDate = DateTime.UtcNow;
                }
            }
            else if (payment.AmountPaid > 0)
            {
                payment.PaymentStatus = PaymentStatus.PartiallyPaid;
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.Pending;
            }
            
            await _paymentCollection.ReplaceOneAsync(
                p => p.Id == payment.Id,
                payment);
        }

        public async Task DeleteAsync(string id)
        {
            var payment = await GetByIdAsync(id);
            if (payment != null)
            {
                payment.Status = false;
                payment.DeletedAt = DateTime.UtcNow;
                await UpdateAsync(payment);
            }
        }

        public async Task<IEnumerable<StudentPayment>> CreateManyAsync(IEnumerable<StudentPayment> payments)
        {
            var paymentsList = new List<StudentPayment>();
            
            foreach (var payment in payments)
            {
                payment.Status = true;
                payment.CreatedAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.Pending = payment.AmountExpense - payment.AmountPaid;
                paymentsList.Add(payment);
            }
            
            await _paymentCollection.InsertManyAsync(paymentsList);
            return paymentsList;
        }

        public async Task UpdateManyAsync(IEnumerable<StudentPayment> payments)
        {
            var bulkOps = new List<WriteModel<StudentPayment>>();
            
            foreach (var payment in payments)
            {
                payment.UpdatedAt = DateTime.UtcNow;
                payment.Pending = payment.AmountExpense - payment.AmountPaid;
                
                // Actualizar el estado del pago basado en los montos
                if (payment.AmountPaid >= payment.AmountExpense)
                {
                    if (payment.AmountPaid > payment.AmountExpense)
                    {
                        payment.Excedent = payment.AmountPaid - payment.AmountExpense;
                        payment.PaymentStatus = PaymentStatus.Excedent;
                    }
                    else
                    {
                        payment.PaymentStatus = PaymentStatus.Paid;
                    }
                    
                    payment.Pending = 0;
                    
                    if (payment.PaymentDate == null)
                    {
                        payment.PaymentDate = DateTime.UtcNow;
                    }
                }
                else if (payment.AmountPaid > 0)
                {
                    payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Pending;
                }
                
                var filter = Builders<StudentPayment>.Filter.Eq(p => p.Id, payment.Id);
                var update = Builders<StudentPayment>.Update
                    .Set(p => p.AmountPaid, payment.AmountPaid)
                    .Set(p => p.PaymentStatus, payment.PaymentStatus)
                    .Set(p => p.Pending, payment.Pending)
                    .Set(p => p.Excedent, payment.Excedent)
                    .Set(p => p.PaymentDate, payment.PaymentDate)
                    .Set(p => p.UpdatedAt, payment.UpdatedAt);
                
                bulkOps.Add(new UpdateOneModel<StudentPayment>(filter, update));
            }
            
            if (bulkOps.Count > 0)
            {
                await _paymentCollection.BulkWriteAsync(bulkOps);
            }
        }
    }
} 