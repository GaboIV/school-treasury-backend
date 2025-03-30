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
        private readonly ILoggerManager _logger;
        private readonly IStudentRepository _studentRepository;
        private readonly ICollectionTypeRepository _collectionTypeRepository;
        private readonly INotificationService _notificationService;

        public CollectionService(
            ICollectionRepository collectionRepository,
            IStudentPaymentRepository studentPaymentRepository,
            IPettyCashService pettyCashService,
            ILoggerManager logger,
            IStudentRepository studentRepository,
            ICollectionTypeRepository collectionTypeRepository,
            INotificationService notificationService)
        {
            _collectionRepository = collectionRepository;
            _studentPaymentRepository = studentPaymentRepository;
            _pettyCashService = pettyCashService;
            _logger = logger;
            _studentRepository = studentRepository;
            _collectionTypeRepository = collectionTypeRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Collection>> GetAllCollectionsAsync()
        {
            _logger.LogInfo("Obteniendo todos los cobros");
            var collections = await _collectionRepository.GetAllAsync();
            _logger.LogInfo($"Se obtuvieron {collections.Count()} cobros correctamente");
            return collections;
        }

        public async Task<Collection> GetCollectionByIdAsync(string id)
        {
            _logger.LogInfo($"Obteniendo cobro con ID: {id}");
            var collection = await _collectionRepository.GetByIdAsync(id);
            if (collection == null)
            {
                _logger.LogWarn($"No se encontró el cobro con ID: {id}");
            }
            else
            {
                _logger.LogInfo($"Cobro encontrado: {collection.Name} con monto total: {collection.TotalAmount}");
            }
            return collection;
        }

        public async Task<Collection> CreateCollectionAsync(CreateCollectionDto dto)
        {
            _logger.LogInfo($"Creando nuevo cobro: {dto.Name} con monto total: {dto.TotalAmount}");
            
            decimal individualAmount = 0;
            
            // Obtener la cantidad real de estudiantes registrados
            int totalStudents = await _studentRepository.CountAsync(s => s.Status == true);
            _logger.LogInfo($"Total de estudiantes registrados en el sistema: {totalStudents}");
            
            if (dto.StudentQuantity == "all") {
                individualAmount = dto.TotalAmount / totalStudents;
                _logger.LogInfo($"Cobro para todos los estudiantes. Monto individual calculado: {individualAmount}");
            }

            var collection = new Collection
            {
                Name = dto.Name,
                CollectionTypeId = dto.CollectionTypeId!,
                Date = dto.Date,
                TotalAmount = dto.TotalAmount,
                IndividualAmount = individualAmount,
                AllowsExemptions = dto.AllowsExemptions,
                Advance = new Advance(),
                StudentQuantity = dto.StudentQuantity
            };

            collection.Advance.Total = totalStudents;
            collection.Advance.Completed = 0;
            collection.Advance.Pending = totalStudents;

            _logger.LogDebug($"Inicializando avance del cobro: Total={totalStudents}, Completados=0, Pendientes={totalStudents}");
            
            await _collectionRepository.InsertAsync(collection);
            _logger.LogInfo($"Cobro creado correctamente con ID: {collection.Id}");
            
            // Crear pagos para cada estudiante
            if (dto.StudentQuantity == "all")
            {
                _logger.LogInfo($"Creando pagos individuales para todos los estudiantes con monto: {individualAmount}");
                await _studentPaymentRepository.CreatePaymentsForCollectionAsync(collection.Id, individualAmount);
                _logger.LogInfo($"Pagos individuales creados correctamente para el cobro con ID: {collection.Id}");
            }

            // Ya no registramos el gasto en la caja chica
            // Los gastos no afectan la caja chica, solo los pagos de estudiantes
            
            // Enviar notificación a los representantes sobre el nuevo cobro
            await SendNewCollectionNotificationAsync(collection);
            
            return collection;
        }

        public async Task<Collection?> UpdateCollectionAsync(UpdateCollectionDto dto)
        {
            _logger.LogInfo($"Actualizando cobro con ID: {dto.Id}");
            
            decimal individualAmount = 0;
            
            // Obtener la cantidad real de estudiantes registrados
            int totalStudents = await _studentRepository.CountAsync(s => s.Status == true);
            _logger.LogDebug($"Total de estudiantes registrados en el sistema: {totalStudents}");

            var existingCollection = await _collectionRepository.GetByIdAsync(dto.Id!);
            
            if (existingCollection == null)
            {
                _logger.LogWarn($"No se encontró el cobro con ID: {dto.Id} para actualizar");
                return null;
            }

            // Guardar el monto total anterior para comparar
            decimal previousTotalAmount = existingCollection.TotalAmount;
            _logger.LogDebug($"Monto total anterior: {previousTotalAmount}, Nuevo monto total: {dto.TotalAmount}");

            if (dto.StudentQuantity == "all") {
                individualAmount = dto.TotalAmount / totalStudents;
                _logger.LogDebug($"Nuevo monto individual calculado: {individualAmount}");
            }

            // Guardar el monto individual anterior para comparar
            decimal previousIndividualAmount = existingCollection.IndividualAmount;
            string previousStudentQuantity = existingCollection.StudentQuantity;
            
            _logger.LogDebug($"Monto individual anterior: {previousIndividualAmount}, Cantidad de estudiantes anterior: {previousStudentQuantity}");

            existingCollection.Name = dto.Name ?? "";
            existingCollection.CollectionTypeId = dto.CollectionTypeId ?? "";
            existingCollection.Date = dto.Date;
            existingCollection.TotalAmount = dto.TotalAmount;
            existingCollection.IndividualAmount = individualAmount;
            existingCollection.StudentQuantity = dto.StudentQuantity;
            existingCollection.Status = dto.Status;
            existingCollection.AllowsExemptions = dto.AllowsExemptions;
            existingCollection.UpdatedAt = DateTime.UtcNow;

            existingCollection.Advance.Total = totalStudents;
            existingCollection.Advance.Pending = totalStudents - existingCollection.Advance.Completed;
            
            _logger.LogDebug($"Actualizando avance del cobro: Total={totalStudents}, Completados={existingCollection.Advance.Completed}, Pendientes={existingCollection.Advance.Pending}");
            
            await _collectionRepository.UpdateAsync(existingCollection);
            _logger.LogInfo($"Cobro actualizado correctamente con ID: {existingCollection.Id}");
            
            // Actualizar pagos de estudiantes si el monto individual cambió
            if (dto.StudentQuantity == "all")
            {
                if (previousStudentQuantity != "all")
                {
                    // Si antes no era para todos los estudiantes, crear los pagos
                    _logger.LogInfo($"Creando pagos individuales para todos los estudiantes con monto: {individualAmount}");
                    await _studentPaymentRepository.CreatePaymentsForCollectionAsync(existingCollection.Id, individualAmount);
                    _logger.LogInfo($"Pagos individuales creados correctamente para el cobro con ID: {existingCollection.Id}");
                }
                else if (previousIndividualAmount != individualAmount)
                {
                    // Si el monto individual cambió, actualizar los pagos existentes
                    _logger.LogInfo($"Actualizando pagos individuales con nuevo monto: {individualAmount}");
                    await _studentPaymentRepository.UpdatePaymentsForCollectionAsync(existingCollection.Id, individualAmount);
                    _logger.LogInfo($"Pagos individuales actualizados correctamente para el cobro con ID: {existingCollection.Id}");
                }
            }

            // Ya no registramos cambios en la caja chica cuando cambia el monto del gasto
            // Los gastos no afectan la caja chica, solo los pagos de estudiantes
            
            return existingCollection;
        }

        public async Task<bool> DeleteCollectionAsync(string id)
        {
            _logger.LogInfo($"Eliminando cobro con ID: {id}");
            
            var existingCollection = await _collectionRepository.GetByIdAsync(id);
            
            if (existingCollection == null)
            {
                _logger.LogWarn($"No se encontró el cobro con ID: {id} para eliminar");
                return false;
            }

            // Verificar si existen gastos asociados a este tipo de gasto
            var existsCollections = await ExistsCollectionWithTypeIdAsync(id);
            if (existsCollections)
            {
                _logger.LogWarn($"No se puede eliminar el cobro con ID: {id} porque tiene cobros asociados");
                return false;
            }

            // Ya no registramos cambios en la caja chica cuando se elimina un gasto
            // Los gastos no afectan la caja chica, solo los pagos de estudiantes

            var result = await _collectionRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInfo($"Cobro eliminado correctamente con ID: {id}");
            }
            else
            {
                _logger.LogError($"Error al eliminar el cobro con ID: {id}");
            }
            return result;
        }

        public async Task<bool> ExistsCollectionWithTypeIdAsync(string collectionTypeId)
        {
            _logger.LogDebug($"Verificando si existen cobros asociados al tipo de cobro con ID: {collectionTypeId}");
            var result = await _collectionRepository.ExistsByCollectionTypeIdAsync(collectionTypeId);
            _logger.LogDebug($"Resultado de verificación para tipo de cobro con ID {collectionTypeId}: {(result ? "Tiene cobros asociados" : "No tiene cobros asociados")}");
            return result;
        }

        public async Task<(IEnumerable<Collection> Collections, int TotalCount)> GetPaginatedCollectionsAsync(int page, int pageSize)
        {
            _logger.LogInfo($"Obteniendo cobros paginados. Página: {page}, Tamaño de página: {pageSize}");
            var result = await _collectionRepository.GetPaginatedAsync(page, pageSize);
            _logger.LogInfo($"Se obtuvieron {result.Items.Count()} cobros de un total de {result.TotalCount}");
            return (result.Items, result.TotalCount);
        }

        public async Task<Collection> AdjustCollectionAmountAsync(string id, AdjustCollectionAmountDto dto)
        {
            _logger.LogInfo($"Ajustando monto del cobro con ID: {id}. Nuevo monto ajustado: {dto.AdjustedAmount}, Excedente: {dto.Surplus}");
            
            var collection = await _collectionRepository.GetByIdAsync(id);
            if (collection == null)
            {
                _logger.LogWarn($"No se encontró el cobro con ID: {id} para ajustar monto");
                throw new KeyNotFoundException($"Cobro con ID {id} no encontrado");
            }

            // Guardar montos anteriores para comparar
            decimal previousAdjustedAmount = collection.AdjustedIndividualAmount ?? collection.IndividualAmount;
            decimal previousTotalAdjustedAmount = previousAdjustedAmount * collection.Advance.Total;
            
            _logger.LogDebug($"Monto ajustado anterior: {previousAdjustedAmount}, Monto total ajustado anterior: {previousTotalAdjustedAmount}");

            // Actualizar el monto ajustado y el excedente total
            collection.AdjustedIndividualAmount = dto.AdjustedAmount;
            collection.TotalSurplus = dto.Surplus;

            // Calcular el nuevo monto total ajustado
            decimal newTotalAdjustedAmount = dto.AdjustedAmount * collection.Advance.Total;
            _logger.LogDebug($"Nuevo monto total ajustado: {newTotalAdjustedAmount}");

            // Obtener todos los pagos relacionados con este gasto
            _logger.LogInfo($"Obteniendo pagos relacionados con el cobro ID: {id}");
            var payments = await _studentPaymentRepository.GetByCollectionIdAsync(id);
            _logger.LogInfo($"Se encontraron {payments.Count()} pagos relacionados con el cobro");

            // Actualizar cada pago
            _logger.LogInfo("Actualizando pagos individuales con el nuevo monto ajustado");
            foreach (var payment in payments)
            {
                _logger.LogDebug($"Actualizando pago ID: {payment.Id} para estudiante: {payment.StudentId}");
                
                // Mantener el monto original
                payment.AmountCollection = collection.IndividualAmount;
                
                // Establecer el monto ajustado
                payment.AdjustedAmountCollection = dto.AdjustedAmount;
                
                // Calcular el excedente individual
                payment.Surplus = dto.Surplus;

                // Recalcular el estado del pago
                string estadoAnterior = payment.PaymentStatus.ToString();
                
                if (payment.AmountPaid >= payment.AdjustedAmountCollection)
                {
                    payment.PaymentStatus = PaymentStatus.Paid;
                    payment.Excedent = payment.AmountPaid - payment.AdjustedAmountCollection;
                    payment.Pending = 0;
                    _logger.LogDebug($"Pago ID: {payment.Id} marcado como Pagado. Excedente: {payment.Excedent}");
                }
                else if (payment.AmountPaid > 0)
                {
                    payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                    payment.Excedent = 0;
                    payment.Pending = payment.AdjustedAmountCollection - payment.AmountPaid;
                    _logger.LogDebug($"Pago ID: {payment.Id} marcado como Parcialmente Pagado. Pendiente: {payment.Pending}");
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Pending;
                    payment.Excedent = 0;
                    payment.Pending = payment.AdjustedAmountCollection;
                    _logger.LogDebug($"Pago ID: {payment.Id} marcado como Pendiente. Pendiente: {payment.Pending}");
                }

                _logger.LogDebug($"Estado del pago ID: {payment.Id} cambió de {estadoAnterior} a {payment.PaymentStatus}");

                // Actualizar el pago en la base de datos
                await _studentPaymentRepository.UpdateAsync(payment);
            }

            // Actualizar el avance del gasto
            int completedPayments = payments.Count(p => p.PaymentStatus == PaymentStatus.Paid);
            collection.Advance.Total = payments.Count();
            collection.Advance.Completed = completedPayments;
            collection.Advance.Pending = collection.Advance.Total - collection.Advance.Completed;
            
            _logger.LogInfo($"Actualizando avance del cobro: Total={collection.Advance.Total}, Completados={collection.Advance.Completed}, Pendientes={collection.Advance.Pending}");

            // Calcular el porcentaje pagado
            var totalPaid = payments.Sum(p => p.AmountPaid);
            var totalAdjusted = (collection.AdjustedIndividualAmount ?? collection.IndividualAmount) * collection.Advance.Total;
            collection.PercentagePaid = totalAdjusted > 0 ? (totalPaid / totalAdjusted) * 100 : 0;
            
            _logger.LogInfo($"Porcentaje pagado del cobro: {collection.PercentagePaid}%. Total pagado: {totalPaid}, Total ajustado: {totalAdjusted}");

            // Ya no registramos cambios en la caja chica cuando cambia el monto ajustado
            // Los gastos no afectan la caja chica, solo los pagos de estudiantes

            // Guardar los cambios en el gasto
            await _collectionRepository.UpdateAsync(collection);
            _logger.LogInfo($"Cobro con ID: {id} actualizado correctamente con el nuevo monto ajustado");

            return collection;
        }

        private async Task SendNewCollectionNotificationAsync(Collection collection)
        {
            try
            {
                _logger.LogInfo($"Enviando notificación de nuevo cobro: {collection.Name}");
                
                // Obtener el tipo de cobro para incluir en la notificación
                var collectionType = await _collectionTypeRepository.GetByIdAsync(collection.CollectionTypeId);
                string typeName = collectionType?.Name ?? "Desconocido";
                
                // Preparar los datos para la notificación
                var notificationData = new
                {
                    CollectionId = collection.Id,
                    CollectionName = collection.Name,
                    CollectionType = typeName,
                    TotalAmount = collection.TotalAmount,
                    IndividualAmount = collection.IndividualAmount,
                    Date = collection.Date
                };
                
                // Enviar notificación a todos los representantes
                var title = "Nuevo cobro registrado";
                var body = $"Se ha registrado un nuevo cobro: {collection.Name} - {typeName}, monto por estudiante: S/ {collection.IndividualAmount:N2}";
                
                // Enviar la notificación al tema de representantes
                await _notificationService.SendNotificationAsync("Representative", title, body, notificationData);
                
                _logger.LogInfo($"Notificación de nuevo cobro enviada exitosamente para el cobro: {collection.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar notificación de nuevo cobro: {ex.Message}");
                // No propagar la excepción para no interrumpir el flujo principal
            }
        }
    }
}

