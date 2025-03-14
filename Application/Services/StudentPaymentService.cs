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
        private readonly ICollectionRepository _collectionRepository;
        private readonly IFileService _fileService;
        private readonly IPettyCashService _pettyCashService;

        public StudentPaymentService(
            IStudentPaymentRepository paymentRepository,
            IStudentRepository studentRepository,
            ICollectionRepository collectionRepository,
            IFileService fileService,
            IPettyCashService pettyCashService)
        {
            _paymentRepository = paymentRepository;
            _studentRepository = studentRepository;
            _collectionRepository = collectionRepository;
            _fileService = fileService;
            _pettyCashService = pettyCashService;
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

        public async Task<IEnumerable<StudentPaymentDto>> GetPaymentsByCollectionIdAsync(string collectionId)
        {
            var payments = await _paymentRepository.GetByCollectionIdAsync(collectionId);
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
            var collection = await _collectionRepository.GetByIdAsync(dto.CollectionId);
            if (collection == null)
            {
                throw new KeyNotFoundException($"Gasto con ID {dto.CollectionId} no encontrado");
            }

            // Verificar si ya existe un pago para este estudiante y gasto
            var existingPayments = await _paymentRepository.GetByCollectionIdAsync(dto.CollectionId);
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
                CollectionId = dto.CollectionId,
                StudentId = dto.StudentId,
                AmountCollection = collection.IndividualAmount,
                AmountPaid = dto.AmountPaid,
                Images = dto.Images,
                Voucher = dto.Voucher,
                Comment = dto.Comment,
                Pending = collection.IndividualAmount - dto.AmountPaid
            };

            var individualAmount = (collection.AdjustedIndividualAmount != null ? collection.AdjustedIndividualAmount : collection.IndividualAmount) ?? 0;

            // Establecer el estado del pago
            if (dto.AmountPaid >= individualAmount)
            {
                if (dto.AmountPaid > individualAmount)
                {
                    payment.Excedent = dto.AmountPaid - individualAmount;
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
            await UpdateCollectionAdvance(collection.Id);
            
            return await EnrichPaymentWithDetails(payment);
        }

        public async Task<StudentPaymentDto> UpdatePaymentAsync(string id, UpdateStudentPaymentDto dto)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Pago con ID {id} no encontrado");
            }

            // Obtener el gasto para verificar si tiene un monto ajustado
            var collection = await _collectionRepository.GetByIdAsync(payment.CollectionId);

            // Guardar monto anterior para comparar
            decimal previousAmountPaid = payment.AmountPaid;

            // Actualizar el pago
            payment.AmountPaid = dto.AmountPaid;
            payment.Voucher = dto.Voucher;
            payment.Comment = dto.Comment;
            payment.UpdatedAt = DateTime.UtcNow;

            // Actualizar imágenes si se proporcionan
            if (dto.Images != null && dto.Images.Any())
            {
                payment.Images = dto.Images;
            }

            // Actualizar el monto ajustado desde el gasto si existe
            if (collection?.AdjustedIndividualAmount.HasValue == true)
            {
                payment.AdjustedAmountCollection = collection.AdjustedIndividualAmount.Value;
                payment.Surplus = collection.TotalSurplus;
            }
            
            // Determinar qué monto usar para las comparaciones (ajustado o original)
            decimal amountToCompare = payment.AdjustedAmountCollection > 0 
                ? payment.AdjustedAmountCollection 
                : payment.AmountCollection;

            // Actualizar el estado del pago
            if (payment.AmountPaid >= amountToCompare)
            {
                payment.PaymentStatus = PaymentStatus.Paid;
                payment.Excedent = payment.AmountPaid - amountToCompare;
                payment.Pending = 0;
                payment.PaymentDate = DateTime.UtcNow;
            }
            else if (payment.AmountPaid > 0)
            {
                payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                payment.Excedent = 0;
                payment.Pending = amountToCompare - payment.AmountPaid;
                payment.PaymentDate = DateTime.UtcNow;
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.Pending;
                payment.Excedent = 0;
                payment.Pending = amountToCompare;
                payment.PaymentDate = null;
            }

            await _paymentRepository.UpdateAsync(payment);

            // Actualizar el avance del gasto
            await UpdateCollectionAdvance(payment.CollectionId);

            // Si es un pago nuevo (antes era 0), registrar un egreso en caja chica
            if (previousAmountPaid == 0 && payment.AmountPaid > 0)
            {
                // Obtener el nombre del estudiante
                var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                string studentName = student?.Name ?? "Desconocido";
                
                await _pettyCashService.RegisterExpenseFromPaymentAsync(
                    payment.Id,
                    payment.AmountPaid,
                    $"Registro de pago para {collection?.Name} - Estudiante: {studentName}"
                );
            }

            // Si hay excedente, registrar un ingreso en caja chica
            if (payment.Excedent > 0)
            {
                // Obtener el nombre del estudiante
                var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                string studentName = student?.Name ?? "Desconocido";
                
                await _pettyCashService.RegisterIncomeFromExcedentAsync(
                    payment.Id,
                    payment.Excedent,
                    $"Excedente de pago para {collection?.Name} - Estudiante: {studentName}"
                );
            }

            return await EnrichPaymentWithDetails(payment);
        }

        public async Task DeletePaymentAsync(string id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Pago con ID {id} no encontrado");
            }

            string collectionId = payment.CollectionId;
            
            await _paymentRepository.DeleteAsync(id);
            
            // Actualizar el avance del gasto
            await UpdateCollectionAdvance(collectionId);
        }

        public async Task<IEnumerable<StudentPaymentDto>> CreatePaymentsForCollectionAsync(string collectionId, decimal individualAmount)
        {
            // Verificar que el gasto existe
            var collection = await _collectionRepository.GetByIdAsync(collectionId);
            if (collection == null)
            {
                throw new KeyNotFoundException($"Gasto con ID {collectionId} no encontrado");
            }

            // Obtener todos los estudiantes
            var students = await _studentRepository.GetAllAsync();
            
            // Crear pagos para cada estudiante
            var payments = new List<StudentPayment>();
            
            foreach (var student in students)
            {
                payments.Add(new StudentPayment
                {
                    CollectionId = collectionId,
                    StudentId = student.Id,
                    AmountCollection = individualAmount,
                    AmountPaid = 0,
                    PaymentStatus = PaymentStatus.Pending,
                    Pending = individualAmount
                });
            }
            
            await _paymentRepository.CreateManyAsync(payments);
            
            // Actualizar el avance del gasto
            collection.Advance.Total = students.Count();
            collection.Advance.Completed = 0;
            collection.Advance.Pending = students.Count();
            await _collectionRepository.UpdateAsync(collection);
            
            return await EnrichPaymentsWithDetails(payments);
        }

        public async Task UpdatePaymentsForCollectionAsync(string collectionId, decimal newIndividualAmount)
        {
            // Verificar que el gasto existe
            var collection = await _collectionRepository.GetByIdAsync(collectionId);
            if (collection == null)
            {
                throw new KeyNotFoundException($"Gasto con ID {collectionId} no encontrado");
            }

            // Obtener todos los pagos para este gasto
            var payments = await _paymentRepository.GetByCollectionIdAsync(collectionId);
            
            // Actualizar el monto individual para cada pago
            foreach (var payment in payments)
            {
                payment.AmountCollection = newIndividualAmount;
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
            await UpdateCollectionAdvance(collectionId);
        }

        public async Task<StudentPaymentDto> RegisterPaymentWithImagesAsync(RegisterPaymentWithImagesDto dto)
        {
            // Verificar que el pago existe
            var payment = await _paymentRepository.GetByIdAsync(dto.Id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Pago con ID {dto.Id} no encontrado");
            }

            // Obtener el gasto para verificar si tiene un monto ajustado
            var collection = await _collectionRepository.GetByIdAsync(payment.CollectionId);

            // Guardar las imágenes en la carpeta específica del pago
            var imagePaths = await _fileService.SaveImagesAsync(dto.Images, $"payments/{dto.Id}");

            // Guardar monto anterior para comparar
            decimal previousAmountPaid = payment.AmountPaid;

            // Actualizar el pago
            payment.AmountPaid = dto.AmountPaid;
            payment.Comment = dto.Comment;
            
            // Añadir las nuevas imágenes a la lista existente
            payment.Images.AddRange(imagePaths);
            
            // Actualizar el monto ajustado desde el gasto si existe
            if (collection?.AdjustedIndividualAmount.HasValue == true)
            {
                payment.AdjustedAmountCollection = collection.AdjustedIndividualAmount.Value;
                payment.Surplus = collection.TotalSurplus;
            }
            
            // Determinar qué monto usar para las comparaciones (ajustado o original)
            decimal amountToCompare = payment.AdjustedAmountCollection > 0 
                ? payment.AdjustedAmountCollection 
                : payment.AmountCollection;
            
            // Calcular el estado del pago basado en el monto apropiado
            if (payment.AmountPaid >= amountToCompare)
            {
                payment.PaymentStatus = PaymentStatus.Paid;
                payment.Excedent = payment.AmountPaid - amountToCompare;
                payment.Pending = 0;
                payment.PaymentDate = DateTime.UtcNow;
            }
            else if (payment.AmountPaid > 0)
            {
                payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                payment.Excedent = 0;
                payment.Pending = amountToCompare - payment.AmountPaid;
                payment.PaymentDate = DateTime.UtcNow;
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.Pending;
                payment.Excedent = 0;
                payment.Pending = amountToCompare;
            }

            // Guardar los cambios
            await _paymentRepository.UpdateAsync(payment);

            // Actualizar el avance del gasto
            await UpdateCollectionAdvance(payment.CollectionId);

            // Si es un pago nuevo (antes era 0), registrar un egreso en caja chica
            if (previousAmountPaid == 0 && payment.AmountPaid > 0)
            {
                // Obtener el nombre del estudiante
                var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                string studentName = student?.Name ?? "Desconocido";
                
                await _pettyCashService.RegisterIncomeFromExcedentAsync(
                    payment.Id,
                    payment.AmountPaid,
                    $"Registro de pago para {collection?.Name} - Estudiante: {studentName}"
                );
            }

            // Si hay excedente, registrar un ingreso en caja chica
            // if (payment.Excedent > 0)
            // {
            //     // Obtener el nombre del estudiante si no se ha obtenido ya
            //     var student = payment.StudentName != null ? null : await _studentRepository.GetByIdAsync(payment.StudentId);
            //     string studentName = payment.StudentName ?? student?.Name ?? "Desconocido";
            //     
            //     await _pettyCashService.RegisterIncomeFromExcedentAsync(
            //         payment.Id,
            //         payment.Excedent,
            //         $"Excedente de pago para {collection?.Name} - Estudiante: {studentName}"
            //     );
            // }
            
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
            var collection = await _collectionRepository.GetByIdAsync(payment.CollectionId);

            // Determinar el monto que se debe mostrar
            decimal amountToShow = payment.AdjustedAmountCollection > 0 
                ? payment.AdjustedAmountCollection 
                : payment.AmountCollection;

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
                CollectionId = payment.CollectionId,
                StudentId = payment.StudentId,
                StudentName = student?.Name,
                CollectionName = collection?.Name,
                AmountCollection = payment.AmountCollection,
                AdjustedAmountCollection = payment.AdjustedAmountCollection,
                AmountPaid = payment.AmountPaid,
                PaymentStatus = payment.PaymentStatus,
                Images = imageUrls,
                Voucher = payment.Voucher,
                Excedent = payment.Excedent,
                Surplus = payment.Surplus,
                Pending = payment.Pending,
                Comment = payment.Comment,
                PaymentDate = payment.PaymentDate,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }

        private async Task UpdateCollectionAdvance(string collectionId)
        {
            var collection = await _collectionRepository.GetByIdAsync(collectionId);
            if (collection == null) return;

            var payments = await _paymentRepository.GetByCollectionIdAsync(collectionId);
            
            // Actualizar el avance
            collection.Advance.Total = payments.Count();
            collection.Advance.Completed = payments.Count(p => p.PaymentStatus == PaymentStatus.Paid);
            collection.Advance.Pending = collection.Advance.Total - collection.Advance.Completed;
            
            // Calcular el porcentaje pagado basado en el monto ajustado si existe
            var totalPaid = payments.Sum(p => p.AmountPaid);
            decimal totalAmount;
            
            if (collection.AdjustedIndividualAmount.HasValue && collection.AdjustedIndividualAmount.Value > 0)
            {
                // Usar el monto ajustado
                totalAmount = collection.AdjustedIndividualAmount.Value * collection.Advance.Total;
            }
            else
            {
                // Usar el monto original
                totalAmount = collection.IndividualAmount * collection.Advance.Total;
            }
            
            collection.PercentagePaid = totalAmount > 0 ? (totalPaid / totalAmount) * 100 : 0;
            
            await _collectionRepository.UpdateAsync(collection);
        }
    }
} 