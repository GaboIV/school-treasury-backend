using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class StudentPaymentService : IStudentPaymentService
    {
        private readonly IStudentPaymentRepository _paymentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IFileService _fileService;

        public StudentPaymentService(
            IStudentPaymentRepository paymentRepository,
            IStudentRepository studentRepository,
            IExpenseRepository expenseRepository,
            IFileService fileService)
        {
            _paymentRepository = paymentRepository;
            _studentRepository = studentRepository;
            _expenseRepository = expenseRepository;
            _fileService = fileService;
        }

        public async Task<IEnumerable<StudentPaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return await EnrichPaymentsWithDetails(payments);
        }

        public async Task<StudentPaymentDto> GetPaymentByIdAsync(string id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Pago con ID {id} no encontrado");
            }

            return await EnrichPaymentWithDetails(payment);
        }

        public async Task<IEnumerable<StudentPaymentDto>> GetPaymentsByStudentIdAsync(string studentId)
        {
            var payments = await _paymentRepository.GetByStudentIdAsync(studentId);
            return await EnrichPaymentsWithDetails(payments);
        }

        public async Task<IEnumerable<StudentPaymentDto>> GetPaymentsByExpenseIdAsync(string expenseId)
        {
            var payments = await _paymentRepository.GetByExpenseIdAsync(expenseId);
            return await EnrichPaymentsWithDetails(payments);
        }

        public async Task<IEnumerable<StudentPaymentDto>> GetPendingPaymentsByStudentIdAsync(string studentId)
        {
            var payments = await _paymentRepository.GetPendingPaymentsByStudentIdAsync(studentId);
            return await EnrichPaymentsWithDetails(payments);
        }

        public async Task<StudentPaymentDto> CreatePaymentAsync(CreateStudentPaymentDto dto)
        {
            // Verificar que el estudiante existe
            var student = await _studentRepository.GetByIdAsync(dto.StudentId);
            if (student == null)
            {
                throw new KeyNotFoundException($"Estudiante con ID {dto.StudentId} no encontrado");
            }

            // Verificar que el gasto existe
            var expense = await _expenseRepository.GetByIdAsync(dto.ExpenseId);
            if (expense == null)
            {
                throw new KeyNotFoundException($"Gasto con ID {dto.ExpenseId} no encontrado");
            }

            // Verificar si ya existe un pago para este estudiante y gasto
            var existingPayments = await _paymentRepository.GetByExpenseIdAsync(dto.ExpenseId);
            var existingPayment = existingPayments.FirstOrDefault(p => p.StudentId == dto.StudentId);

            if (existingPayment != null)
            {
                // Actualizar el pago existente
                existingPayment.AmountPaid += dto.AmountPaid;
                
                if (dto.Images != null && dto.Images.Count > 0)
                {
                    existingPayment.Images.AddRange(dto.Images);
                }
                
                if (!string.IsNullOrEmpty(dto.Voucher))
                {
                    existingPayment.Voucher = dto.Voucher;
                }
                
                if (!string.IsNullOrEmpty(dto.Comment))
                {
                    existingPayment.Comment = dto.Comment;
                }

                await _paymentRepository.UpdateAsync(existingPayment);
                return await EnrichPaymentWithDetails(existingPayment);
            }

            // Crear un nuevo pago
            var payment = new StudentPayment
            {
                ExpenseId = dto.ExpenseId,
                StudentId = dto.StudentId,
                AmountExpense = expense.IndividualAmount,
                AmountPaid = dto.AmountPaid,
                Images = dto.Images,
                Voucher = dto.Voucher,
                Comment = dto.Comment,
                Pending = expense.IndividualAmount - dto.AmountPaid
            };

            // Establecer el estado del pago
            if (dto.AmountPaid >= expense.IndividualAmount)
            {
                if (dto.AmountPaid > expense.IndividualAmount)
                {
                    payment.Excedent = dto.AmountPaid - expense.IndividualAmount;
                    payment.PaymentStatus = PaymentStatus.Excedent;
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Paid;
                }
                
                payment.Pending = 0;
                payment.PaymentDate = DateTime.UtcNow;
            }
            else if (dto.AmountPaid > 0)
            {
                payment.PaymentStatus = PaymentStatus.PartiallyPaid;
            }

            await _paymentRepository.CreateAsync(payment);
            
            // Actualizar el avance del gasto
            await UpdateExpenseAdvance(expense.Id);
            
            return await EnrichPaymentWithDetails(payment);
        }

        public async Task<StudentPaymentDto> UpdatePaymentAsync(string id, UpdateStudentPaymentDto dto)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Pago con ID {id} no encontrado");
            }

            // Actualizar los campos
            payment.AmountPaid = dto.AmountPaid;
            
            if (dto.Images != null && dto.Images.Count > 0)
            {
                payment.Images = dto.Images;
            }
            
            if (dto.Voucher != null)
            {
                payment.Voucher = dto.Voucher;
            }
            
            if (dto.Comment != null)
            {
                payment.Comment = dto.Comment;
            }

            await _paymentRepository.UpdateAsync(payment);
            
            // Actualizar el avance del gasto
            await UpdateExpenseAdvance(payment.ExpenseId);
            
            return await EnrichPaymentWithDetails(payment);
        }

        public async Task DeletePaymentAsync(string id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Pago con ID {id} no encontrado");
            }

            string expenseId = payment.ExpenseId;
            
            await _paymentRepository.DeleteAsync(id);
            
            // Actualizar el avance del gasto
            await UpdateExpenseAdvance(expenseId);
        }

        public async Task<IEnumerable<StudentPaymentDto>> CreatePaymentsForExpenseAsync(string expenseId, decimal individualAmount)
        {
            // Verificar que el gasto existe
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                throw new KeyNotFoundException($"Gasto con ID {expenseId} no encontrado");
            }

            // Obtener todos los estudiantes
            var students = await _studentRepository.GetAllAsync();
            
            // Crear pagos para cada estudiante
            var payments = new List<StudentPayment>();
            
            foreach (var student in students)
            {
                payments.Add(new StudentPayment
                {
                    ExpenseId = expenseId,
                    StudentId = student.Id,
                    AmountExpense = individualAmount,
                    AmountPaid = 0,
                    PaymentStatus = PaymentStatus.Pending,
                    Pending = individualAmount
                });
            }
            
            await _paymentRepository.CreateManyAsync(payments);
            
            // Actualizar el avance del gasto
            expense.Advance.Total = students.Count();
            expense.Advance.Completed = 0;
            expense.Advance.Pending = students.Count();
            await _expenseRepository.UpdateAsync(expense);
            
            return await EnrichPaymentsWithDetails(payments);
        }

        public async Task UpdatePaymentsForExpenseAsync(string expenseId, decimal newIndividualAmount)
        {
            // Verificar que el gasto existe
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                throw new KeyNotFoundException($"Gasto con ID {expenseId} no encontrado");
            }

            // Obtener todos los pagos para este gasto
            var payments = await _paymentRepository.GetByExpenseIdAsync(expenseId);
            
            // Actualizar el monto individual para cada pago
            foreach (var payment in payments)
            {
                payment.AmountExpense = newIndividualAmount;
                payment.Pending = newIndividualAmount - payment.AmountPaid;
                
                // Actualizar el estado del pago
                if (payment.AmountPaid >= newIndividualAmount)
                {
                    if (payment.AmountPaid > newIndividualAmount)
                    {
                        payment.Excedent = payment.AmountPaid - newIndividualAmount;
                        payment.PaymentStatus = PaymentStatus.Excedent;
                    }
                    else
                    {
                        payment.PaymentStatus = PaymentStatus.Paid;
                        payment.Excedent = 0;
                    }
                    
                    payment.Pending = 0;
                }
                else if (payment.AmountPaid > 0)
                {
                    payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                    payment.Excedent = 0;
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Pending;
                    payment.Excedent = 0;
                }
            }
            
            await _paymentRepository.UpdateManyAsync(payments);
            
            // Actualizar el avance del gasto
            await UpdateExpenseAdvance(expenseId);
        }

        public async Task<StudentPaymentDto> RegisterPaymentWithImagesAsync(RegisterPaymentWithImagesDto dto)
        {
            // Verificar que el pago existe
            var payment = await _paymentRepository.GetByIdAsync(dto.Id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Pago con ID {dto.Id} no encontrado");
            }

            // Guardar las imágenes en la carpeta específica del pago
            var imagePaths = await _fileService.SaveImagesAsync(dto.Images, $"payments/{dto.Id}");

            // Actualizar el pago
            payment.AmountPaid = dto.AmountPaid;
            payment.Comment = dto.Comment;
            
            // Añadir las nuevas imágenes a la lista existente
            payment.Images.AddRange(imagePaths);
            
            // Calcular el estado del pago
            if (payment.AmountPaid >= payment.AmountExpense)
            {
                payment.PaymentStatus = PaymentStatus.Paid;
                payment.Excedent = payment.AmountPaid - payment.AmountExpense;
                payment.Pending = 0;
                payment.PaymentDate = DateTime.UtcNow;
            }
            else if (payment.AmountPaid > 0)
            {
                payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                payment.Excedent = 0;
                payment.Pending = payment.AmountExpense - payment.AmountPaid;
                payment.PaymentDate = DateTime.UtcNow;
            }

            // Guardar los cambios
            await _paymentRepository.UpdateAsync(payment);

            // Actualizar el avance del gasto
            await UpdateExpenseAdvance(payment.ExpenseId);

            // Retornar el DTO enriquecido
            return await EnrichPaymentWithDetails(payment);
        }

        private async Task<IEnumerable<StudentPaymentDto>> EnrichPaymentsWithDetails(IEnumerable<StudentPayment> payments)
        {
            var result = new List<StudentPaymentDto>();
            
            foreach (var payment in payments)
            {
                result.Add(await EnrichPaymentWithDetails(payment));
            }
            
            return result;
        }

        private async Task<StudentPaymentDto> EnrichPaymentWithDetails(StudentPayment payment)
        {
            var student = await _studentRepository.GetByIdAsync(payment.StudentId);
            var expense = await _expenseRepository.GetByIdAsync(payment.ExpenseId);

            var imageUrls = new List<string>();
            foreach (var imagePath in payment.Images)
            {
                if (_fileService.ImageExists(imagePath))
                {
                    imageUrls.Add(_fileService.GetImageUrl(imagePath));
                }
            }

            return new StudentPaymentDto
            {
                Id = payment.Id,
                ExpenseId = payment.ExpenseId,
                StudentId = payment.StudentId,
                StudentName = student?.Name,
                ExpenseName = expense?.Name,
                AmountExpense = payment.AmountExpense,
                AmountPaid = payment.AmountPaid,
                PaymentStatus = payment.PaymentStatus,
                Images = imageUrls,
                Voucher = payment.Voucher,
                Excedent = payment.Excedent,
                Pending = payment.Pending,
                Comment = payment.Comment,
                PaymentDate = payment.PaymentDate,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }

        private async Task UpdateExpenseAdvance(string expenseId)
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null) return;
            
            var payments = await _paymentRepository.GetByExpenseIdAsync(expenseId);
            
            // Contar pagos completados
            int completedPayments = payments.Count(p => 
                p.PaymentStatus == PaymentStatus.Paid || 
                p.PaymentStatus == PaymentStatus.Excedent);
            
            // Actualizar el avance
            expense.Advance.Completed = completedPayments;
            expense.Advance.Pending = expense.Advance.Total - completedPayments;
            
            await _expenseRepository.UpdateAsync(expense);
        }
    }
} 