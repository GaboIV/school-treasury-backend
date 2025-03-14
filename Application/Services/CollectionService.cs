using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services {
    public class CollectionService : ICollectionService
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IStudentPaymentRepository _studentPaymentRepository;
        private readonly IPettyCashService _pettyCashService;

        public CollectionService(
            ICollectionRepository collectionRepository,
            IStudentPaymentRepository studentPaymentRepository,
            IPettyCashService pettyCashService)
        {
            _collectionRepository = collectionRepository;
            _studentPaymentRepository = studentPaymentRepository;
            _pettyCashService = pettyCashService;
        }

        public async Task<IEnumerable<Collection>> GetAllCollectionsAsync()
        {
            var collections = await _collectionRepository.GetAllAsync();
            foreach(var collection in collections)
            {
                Console.WriteLine("Estado: " + collection);
            }
            return collections;
        }

        public async Task<Collection> GetCollectionByIdAsync(string id)
        {
            return await _collectionRepository.GetByIdAsync(id);
        }

        public async Task<Collection> CreateCollectionAsync(CreateCollectionDto dto)
        {
            decimal individualAmount = 0;
            int totalStudents = 24;
            
            if (dto.StudentQuantity == "all") {
                individualAmount = dto.TotalAmount / totalStudents;
            }

            var collection = new Collection
            {
                Name = dto.Name,
                CollectionTypeId = dto.CollectionTypeId!,
                Date = dto.Date,
                TotalAmount = dto.TotalAmount,
                IndividualAmount = individualAmount,
                Advance = new Advance(),
                StudentQuantity = dto.StudentQuantity
            };

            collection.Advance.Total = totalStudents;
            collection.Advance.Completed = 0;
            collection.Advance.Pending = totalStudents;

            await _collectionRepository.InsertAsync(collection);
            
            // Crear pagos para cada estudiante
            if (dto.StudentQuantity == "all")
            {
                await _studentPaymentRepository.CreatePaymentsForCollectionAsync(collection.Id, individualAmount);
            }

            // Ya no registramos el gasto en la caja chica
            // Los gastos no afectan la caja chica, solo los pagos de estudiantes
            
            return collection;
        }

        public async Task<Collection?> UpdateCollectionAsync(UpdateCollectionDto dto)
        {
            decimal individualAmount = 0;
            int totalStudents = 24;

            var existingCollection = await _collectionRepository.GetByIdAsync(dto.Id!);
            
            if (existingCollection == null)
                return null;

            // Guardar el monto total anterior para comparar
            decimal previousTotalAmount = existingCollection.TotalAmount;

            if (dto.StudentQuantity == "all") {
                individualAmount = dto.TotalAmount / totalStudents;
            }

            // Guardar el monto individual anterior para comparar
            decimal previousIndividualAmount = existingCollection.IndividualAmount;
            string previousStudentQuantity = existingCollection.StudentQuantity;

            existingCollection.Name = dto.Name ?? "";
            existingCollection.CollectionTypeId = dto.CollectionTypeId ?? "";
            existingCollection.Date = dto.Date;
            existingCollection.TotalAmount = dto.TotalAmount;
            existingCollection.IndividualAmount = individualAmount;
            existingCollection.StudentQuantity = dto.StudentQuantity;
            existingCollection.Status = dto.Status;
            existingCollection.UpdatedAt = DateTime.UtcNow;

            existingCollection.Advance.Total = totalStudents;
            existingCollection.Advance.Pending = totalStudents - existingCollection.Advance.Completed;
            
            await _collectionRepository.UpdateAsync(existingCollection);
            
            // Actualizar pagos de estudiantes si el monto individual cambió
            if (dto.StudentQuantity == "all")
            {
                if (previousStudentQuantity != "all")
                {
                    // Si antes no era para todos los estudiantes, crear los pagos
                    await _studentPaymentRepository.CreatePaymentsForCollectionAsync(existingCollection.Id, individualAmount);
                }
                else if (previousIndividualAmount != individualAmount)
                {
                    // Si el monto individual cambió, actualizar los pagos existentes
                    await _studentPaymentRepository.UpdatePaymentsForCollectionAsync(existingCollection.Id, individualAmount);
                }
            }

            // Ya no registramos cambios en la caja chica cuando cambia el monto del gasto
            // Los gastos no afectan la caja chica, solo los pagos de estudiantes
            
            return existingCollection;
        }

        public async Task<bool> DeleteCollectionAsync(string id)
        {
            var existingCollection = await _collectionRepository.GetByIdAsync(id);
            
            if (existingCollection == null)
                return false;

            // Verificar si existen gastos asociados a este tipo de gasto
            var existsCollections = await ExistsCollectionWithTypeIdAsync(id);
            if (existsCollections)
                return false;

            // Ya no registramos cambios en la caja chica cuando se elimina un gasto
            // Los gastos no afectan la caja chica, solo los pagos de estudiantes

            return await _collectionRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsCollectionWithTypeIdAsync(string collectionTypeId)
        {
            return await _collectionRepository.ExistsByCollectionTypeIdAsync(collectionTypeId);
        }

        public async Task<(IEnumerable<Collection> Collections, int TotalCount)> GetPaginatedCollectionsAsync(int page, int pageSize)
        {
            var result = await _collectionRepository.GetPaginatedAsync(page, pageSize);
            return (result.Items, result.TotalCount);
        }

        public async Task<Collection> AdjustCollectionAmountAsync(string id, AdjustCollectionAmountDto dto)
        {
            var collection = await _collectionRepository.GetByIdAsync(id);
            if (collection == null)
            {
                throw new KeyNotFoundException($"Gasto con ID {id} no encontrado");
            }

            // Guardar montos anteriores para comparar
            decimal previousAdjustedAmount = collection.AdjustedIndividualAmount ?? collection.IndividualAmount;
            decimal previousTotalAdjustedAmount = previousAdjustedAmount * collection.Advance.Total;

            // Actualizar el monto ajustado y el excedente total
            collection.AdjustedIndividualAmount = dto.AdjustedAmount;
            collection.TotalSurplus = dto.Surplus;

            // Calcular el nuevo monto total ajustado
            decimal newTotalAdjustedAmount = dto.AdjustedAmount * collection.Advance.Total;

            // Obtener todos los pagos relacionados con este gasto
            var payments = await _studentPaymentRepository.GetByCollectionIdAsync(id);

            // Actualizar cada pago
            foreach (var payment in payments)
            {
                // Mantener el monto original
                payment.AmountCollection = collection.IndividualAmount;
                
                // Establecer el monto ajustado
                payment.AdjustedAmountCollection = dto.AdjustedAmount;
                
                // Calcular el excedente individual
                payment.Surplus = dto.Surplus;

                // Recalcular el estado del pago
                if (payment.AmountPaid >= payment.AdjustedAmountCollection)
                {
                    payment.PaymentStatus = PaymentStatus.Paid;
                    payment.Excedent = payment.AmountPaid - payment.AdjustedAmountCollection;
                    payment.Pending = 0;
                }
                else if (payment.AmountPaid > 0)
                {
                    payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                    payment.Excedent = 0;
                    payment.Pending = payment.AdjustedAmountCollection - payment.AmountPaid;
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Pending;
                    payment.Excedent = 0;
                    payment.Pending = payment.AdjustedAmountCollection;
                }

                // Actualizar el pago en la base de datos
                await _studentPaymentRepository.UpdateAsync(payment);
            }

            // Actualizar el avance del gasto
            collection.Advance.Total = payments.Count();
            collection.Advance.Completed = payments.Count(p => p.PaymentStatus == PaymentStatus.Paid);
            collection.Advance.Pending = collection.Advance.Total - collection.Advance.Completed;

            // Calcular el porcentaje pagado
            var totalPaid = payments.Sum(p => p.AmountPaid);
            var totalAdjusted = (collection.AdjustedIndividualAmount ?? collection.IndividualAmount) * collection.Advance.Total;
            collection.PercentagePaid = totalAdjusted > 0 ? (totalPaid / totalAdjusted) * 100 : 0;

            // Ya no registramos cambios en la caja chica cuando cambia el monto ajustado
            // Los gastos no afectan la caja chica, solo los pagos de estudiantes

            // Guardar los cambios en el gasto
            await _collectionRepository.UpdateAsync(collection);

            return collection;
        }
    }
}

