using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class StudentPaymentRepository : IStudentPaymentRepository
    {
        private readonly IMongoCollection<StudentPayment> _paymentCollection;
        private readonly IMongoCollection<Student> _studentCollection;

        public StudentPaymentRepository(IMongoDatabase database)
        {
            _paymentCollection = database.GetCollection<StudentPayment>("StudentPayments");
            _studentCollection = database.GetCollection<Student>("Students");
        }

        public async Task<IEnumerable<StudentPayment>> GetAllAsync()
        {
            return await _paymentCollection.Find(payment => payment.Status == true).ToListAsync();
        }

        public async Task<StudentPayment?> GetByIdAsync(string id)
        {
            return await _paymentCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<StudentPayment>> GetByStudentIdAsync(string studentId)
        {
            return await _paymentCollection.Find(payment => payment.StudentId == studentId && payment.Status == true).ToListAsync();
        }

        public async Task<IEnumerable<StudentPayment>> GetByCollectionIdAsync(string collectionId)
        {
            return await _paymentCollection.Find(payment => payment.CollectionId == collectionId && payment.Status == true).ToListAsync();
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
            payment.Pending = payment.AmountCollection - payment.AmountPaid;
            
            await _paymentCollection.InsertOneAsync(payment);
            return payment;
        }

        public async Task UpdateAsync(StudentPayment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            payment.Pending = payment.AmountCollection - payment.AmountPaid;
            
            await _paymentCollection.ReplaceOneAsync(
                p => p.Id == payment.Id,
                payment);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _paymentCollection.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<StudentPayment>> CreateManyAsync(IEnumerable<StudentPayment> payments)
        {
            var paymentsList = new List<StudentPayment>();
            
            foreach (var payment in payments)
            {
                payment.Status = true;
                payment.CreatedAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.Pending = payment.AmountCollection - payment.AmountPaid;
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
                payment.Pending = payment.AmountCollection - payment.AmountPaid;
                
                // Actualizar el estado del pago basado en los montos
                if (payment.AmountPaid >= payment.AmountCollection)
                {
                    if (payment.AmountPaid > payment.AmountCollection)
                    {
                        payment.Excedent = payment.AmountPaid - payment.AmountCollection;
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

        public async Task CreatePaymentsForCollectionAsync(string collectionId, decimal individualAmount)
        {
            var students = await _studentCollection.Find(_ => true).ToListAsync();
            var payments = students.Select(student => new StudentPayment
            {
                CollectionId = collectionId,
                StudentId = student.Id,
                AmountCollection = individualAmount,
                AdjustedAmountCollection = individualAmount,
                PaymentStatus = PaymentStatus.Pending,
                Pending = individualAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _paymentCollection.InsertManyAsync(payments);
        }

        public async Task UpdatePaymentsForCollectionAsync(string collectionId, decimal newIndividualAmount)
        {
            var payments = await GetByCollectionIdAsync(collectionId);
            foreach (var payment in payments)
            {
                payment.AmountCollection = newIndividualAmount;
                payment.AdjustedAmountCollection = newIndividualAmount;
                payment.Pending = newIndividualAmount - payment.AmountPaid;
                payment.UpdatedAt = DateTime.UtcNow;

                if (payment.AmountPaid >= newIndividualAmount)
                {
                    payment.PaymentStatus = PaymentStatus.Paid;
                    payment.Excedent = payment.AmountPaid - newIndividualAmount;
                    payment.Pending = 0;
                }
                else if (payment.AmountPaid > 0)
                {
                    payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                    payment.Excedent = 0;
                }

                await UpdateAsync(payment);
            }
        }

        public async Task InsertAsync(StudentPayment payment)
        {
            await _paymentCollection.InsertOneAsync(payment);
        }
    }
} 