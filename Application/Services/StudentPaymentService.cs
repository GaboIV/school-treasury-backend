using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Application.Services
{
    public class StudentPaymentService : IStudentPaymentService
    {
        private readonly IStudentPaymentRepository _paymentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICollectionRepository _collectionRepository;
        private readonly IFileService _fileService;
        private readonly IPettyCashService _pettyCashService;
        private readonly Tracer _tracer;
        private readonly ILogger<StudentPaymentService> _logger;
        private readonly INotificationService _notificationService;
        private static readonly ActivitySource _activitySource = new("SchoolTreasure.PaymentService");

        public StudentPaymentService(
            IStudentPaymentRepository paymentRepository,
            IStudentRepository studentRepository,
            ICollectionRepository collectionRepository,
            IFileService fileService,
            IPettyCashService pettyCashService,
            INotificationService notificationService,
            ILogger<StudentPaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _studentRepository = studentRepository;
            _collectionRepository = collectionRepository;
            _fileService = fileService;
            _pettyCashService = pettyCashService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IEnumerable<StudentPaymentDto>> GetAllPaymentsAsync()
        {
            using var activity = _activitySource.StartActivity("GetAllPayments");
            _logger.LogInformation("Obteniendo todos los pagos");
            
            try
            {
                var payments = await _paymentRepository.GetAllAsync();
                _logger.LogInformation("Se encontraron {Count} pagos", payments.Count());
                return await EnrichPaymentsWithDetails(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los pagos");
                throw;
            }
        }

        public async Task<StudentPaymentDto> GetPaymentByIdAsync(string id)
        {
            using var activity = _activitySource.StartActivity("GetPaymentById");
            activity?.SetTag("payment.id", id);
            
            _logger.LogInformation("Buscando pago con ID: {PaymentId}", id);
            
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(id);
                if (payment == null)
                {
                    _logger.LogWarning("Pago no encontrado con ID: {PaymentId}", id);
                    throw new KeyNotFoundException($"Pago con ID {id} no encontrado");
                }

                _logger.LogInformation("Pago encontrado con ID: {PaymentId}", id);
                return await EnrichPaymentWithDetails(payment);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error al obtener pago con ID: {PaymentId}", id);
                throw;
            }
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
            using var activity = _activitySource.StartActivity("CreatePayment");
            activity?.SetTag("student.id", dto.StudentId);
            activity?.SetTag("collection.id", dto.CollectionId);
            
            _logger.LogInformation("Iniciando creación de pago para estudiante {StudentId} en colección {CollectionId}", 
                dto.StudentId, dto.CollectionId);

            try
            {
                // Verificar que el estudiante existe
                var student = await _studentRepository.GetByIdAsync(dto.StudentId);
                if (student == null)
                {
                    _logger.LogWarning("Estudiante no encontrado con ID: {StudentId}", dto.StudentId);
                    throw new KeyNotFoundException($"Estudiante con ID {dto.StudentId} no encontrado");
                }

                // Verificar que el gasto existe
                var collection = await _collectionRepository.GetByIdAsync(dto.CollectionId);
                if (collection == null)
                {
                    _logger.LogWarning("Gasto no encontrado con ID: {CollectionId}", dto.CollectionId);
                    throw new KeyNotFoundException($"Gasto con ID {dto.CollectionId} no encontrado");
                }

                _logger.LogDebug("Verificando pagos existentes para estudiante {StudentId} en colección {CollectionId}",
                    dto.StudentId, dto.CollectionId);

                // Verificar si ya existe un pago
                var existingPayments = await _paymentRepository.GetByCollectionIdAsync(dto.CollectionId);
                var existingPayment = existingPayments.FirstOrDefault(p => p.StudentId == dto.StudentId);

                if (existingPayment != null)
                {
                    _logger.LogInformation("Actualizando pago existente para estudiante {StudentId}", dto.StudentId);
                    
                    decimal previousAmount = existingPayment.AmountPaid;
                    existingPayment.AmountPaid += dto.AmountPaid;
                    
                    _logger.LogDebug("Monto anterior: {PreviousAmount}, Nuevo monto: {NewAmount}", 
                        previousAmount, existingPayment.AmountPaid);

                    if (dto.Images != null && dto.Images.Count > 0)
                    {
                        _logger.LogDebug("Agregando {Count} imágenes al pago", dto.Images.Count);
                        existingPayment.Images.AddRange(dto.Images);
                    }
                    
                    await _paymentRepository.UpdateAsync(existingPayment);
                    
                    var updatedPayment = await EnrichPaymentWithDetails(existingPayment);
                    
                    // Enviar notificación si el pago anterior era 0 (primer pago)
                    if (previousAmount == 0 && dto.AmountPaid > 0)
                    {
                        await SendPaymentNotification(updatedPayment, student.Name, collection.Name);
                    }
                    
                    return updatedPayment;
                }

                _logger.LogInformation("Creando nuevo pago para estudiante {StudentId}", dto.StudentId);

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
                    Pending = collection.IndividualAmount - dto.AmountPaid,
                    PaymentDate = dto.PaymentDate
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
                    
                    // Si no se proporcionó una fecha, se establece automáticamente
                    if (payment.PaymentDate == null)
                    {
                        payment.PaymentDate = DateTime.UtcNow;
                    }
                }
                else if (dto.AmountPaid > 0)
                {
                    payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                    
                    // Si no se proporcionó una fecha, se establece automáticamente
                    if (payment.PaymentDate == null)
                    {
                        payment.PaymentDate = DateTime.UtcNow;
                    }
                }

                await _paymentRepository.CreateAsync(payment);
                
                // Actualizar el avance del gasto
                await UpdateCollectionAdvance(collection.Id);
                
                var result = await EnrichPaymentWithDetails(payment);
                
                // Enviar notificación si el monto es mayor a 0
                if (dto.AmountPaid > 0)
                {
                    await SendPaymentNotification(result, student.Name, collection.Name);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pago para estudiante {StudentId} en colección {CollectionId}", 
                    dto.StudentId, dto.CollectionId);
                throw;
            }
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
            
            // Actualizar la fecha de pago si se proporciona
            if (dto.PaymentDate.HasValue)
            {
                payment.PaymentDate = dto.PaymentDate;
            }

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
                
                // Si no tiene fecha de pago, establecerla
                if (payment.PaymentDate == null)
                {
                    payment.PaymentDate = DateTime.UtcNow;
                }
            }
            else if (payment.AmountPaid > 0)
            {
                payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                payment.Excedent = 0;
                payment.Pending = amountToCompare - payment.AmountPaid;
                
                // Si no tiene fecha de pago, establecerla
                if (payment.PaymentDate == null)
                {
                    payment.PaymentDate = DateTime.UtcNow;
                }
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.Pending;
                payment.Excedent = 0;
                payment.Pending = amountToCompare;
                // Si el monto es 0, anular la fecha de pago
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
            using var activity = _activitySource.StartActivity("RegisterPaymentWithImages");
            var stopwatch = Stopwatch.StartNew();  // ⏱ Inicia medición de tiempo
            
            _logger.LogInformation("Service: Iniciando registro de pago con imágenes para PaymentId: {PaymentId}", dto.Id);
            
            // Verificar que el pago existe
            var payment = await _paymentRepository.GetByIdAsync(dto.Id);
            if (payment == null)
            {
                _logger.LogWarning("Service: Pago con ID {PaymentId} no encontrado", dto.Id);
                throw new KeyNotFoundException($"Pago con ID {dto.Id} no encontrado");
            }
            
            _logger.LogInformation("Service: Pago encontrado con ID {PaymentId}", payment.Id);
            
            // Obtener el gasto para verificar si tiene un monto ajustado
            var collection = await _collectionRepository.GetByIdAsync(payment.CollectionId);
            _logger.LogInformation("Service: Cobro obtenida con ID {CollectionId}", payment.CollectionId);
            
            // Guardar las imágenes en la carpeta específica del pago
            var imagePaths = await _fileService.SaveImagesAsync(dto.Images, $"payments/{dto.Id}");
            _logger.LogInformation("Service: {ImageCount} imágenes guardadas para PaymentId: {PaymentId}", imagePaths.Count, dto.Id);
            
            // Guardar monto anterior para comparar
            decimal previousAmountPaid = payment.AmountPaid;
            _logger.LogInformation("Service: Monto previo del pago: {PreviousAmountPaid}", previousAmountPaid);

            // Actualizar el pago con el nuevo monto
            payment.AmountPaid = dto.AmountPaid;
            payment.Comment = dto.Comment;
            payment.Images.AddRange(imagePaths);
            
            // Actualizar la fecha de pago si se proporciona
            if (dto.PaymentDate.HasValue)
            {
                payment.PaymentDate = dto.PaymentDate;
            }
            
            _logger.LogInformation("Service: Monto actualizado a {AmountPaid} para PaymentId: {PaymentId}", dto.AmountPaid, dto.Id);

            // Comparación de montos antes de definir el estado
            _logger.LogInformation("Service: Comparando montos - Anterior: {PreviousAmountPaid}, Nuevo: {AmountPaid}", previousAmountPaid, dto.AmountPaid);

            // Determinar el monto de comparación
            decimal amountToCompare = payment.AdjustedAmountCollection > 0 ? payment.AdjustedAmountCollection : payment.AmountCollection;
            _logger.LogInformation("Service: Monto de comparación determinado: {AmountToCompare}", amountToCompare);

            // Comparación del monto pagado con el monto de referencia
            _logger.LogInformation("Service: Monto a comparar: {AmountToCompare}, Monto pagado: {AmountPaid}", amountToCompare, payment.AmountPaid);

            // Evaluar el estado del pago
            if (payment.AmountPaid >= amountToCompare)
            {
                payment.PaymentStatus = PaymentStatus.Paid;
                payment.Excedent = payment.AmountPaid - amountToCompare;
                payment.Pending = 0;
                
                // Si no tiene fecha de pago, establecerla
                if (payment.PaymentDate == null)
                {
                    payment.PaymentDate = DateTime.UtcNow;
                }
                
                _logger.LogInformation("Service: Pago completado. Excedente: {Excedent}, Estado: {PaymentStatus}", payment.Excedent, payment.PaymentStatus);
            }
            else if (payment.AmountPaid > 0)
            {
                payment.PaymentStatus = PaymentStatus.PartiallyPaid;
                payment.Excedent = 0;
                payment.Pending = amountToCompare - payment.AmountPaid;
                
                // Si no tiene fecha de pago, establecerla
                if (payment.PaymentDate == null)
                {
                    payment.PaymentDate = DateTime.UtcNow;
                }
                
                _logger.LogInformation("Service: Pago parcial. Pendiente: {Pending}, Estado: {PaymentStatus}", payment.Pending, payment.PaymentStatus);
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.Pending;
                payment.Excedent = 0;
                payment.Pending = amountToCompare;
                // Si el monto es 0, anular la fecha de pago
                payment.PaymentDate = null;
                _logger.LogInformation("Service: Pago pendiente. Pendiente: {Pending}, Estado: {PaymentStatus}", payment.Pending, payment.PaymentStatus);
            }
            
            // Guardar los cambios
            await _paymentRepository.UpdateAsync(payment);
            _logger.LogInformation("Service: Pago actualizado en la base de datos para PaymentId: {PaymentId}", payment.Id);
            
            // Actualizar el avance del gasto
            await UpdateCollectionAdvance(payment.CollectionId);
            _logger.LogInformation("Service: Avance de colección actualizado para CollectionId: {CollectionId}", payment.CollectionId);
            
            // Si es un pago nuevo (antes era 0), registrar un egreso en caja chica
            if (previousAmountPaid == 0 && payment.AmountPaid > 0)
            {
                var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                string studentName = student?.Name ?? "Desconocido";
                
                _logger.LogInformation("Service: Registrando ingreso en caja chica para PaymentId: {PaymentId}", payment.Id);
                
                await _pettyCashService.RegisterIncomeFromExcedentAsync(
                    payment.Id,
                    payment.AmountPaid,
                    $"Registro de pago para {collection?.Name} - Estudiante: {studentName}"
                );
                
                // Enviar notificación del nuevo pago
                var enrichedPayment = await EnrichPaymentWithDetails(payment);
                await SendPaymentNotification(enrichedPayment, studentName, collection?.Name ?? "Desconocido");
            }
            
            // Retornar el DTO enriquecido
            var result = await EnrichPaymentWithDetails(payment);
            _logger.LogInformation("Service: Pago registrado exitosamente para PaymentId: {PaymentId}", payment.Id);
            return result;
        }

        public async Task<StudentPaymentDto> ExoneratePaymentAsync(string id, ExoneratePaymentDto dto)
        {
            using var activity = _activitySource.StartActivity("ExoneratePayment");
            var stopwatch = Stopwatch.StartNew();
            
            _logger.LogInformation("Service: Iniciando exoneración de pago para PaymentId: {PaymentId}", id);
            
            // Verificar que el pago existe
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                _logger.LogWarning("Service: Pago con ID {PaymentId} no encontrado", id);
                throw new KeyNotFoundException($"Pago con ID {id} no encontrado");
            }
            
            _logger.LogInformation("Service: Pago encontrado con ID {PaymentId}", payment.Id);
            
            // Obtener el gasto para verificar si tiene un monto ajustado
            var collection = await _collectionRepository.GetByIdAsync(payment.CollectionId);
            _logger.LogInformation("Service: Cobro obtenido con ID {CollectionId}", payment.CollectionId);
            
            // Verificar si el cobro permite exoneraciones
            if (collection == null || collection.AllowsExemptions != true)
            {
                _logger.LogWarning("Service: El cobro con ID {CollectionId} no permite exoneraciones", payment.CollectionId);
                throw new InvalidOperationException($"El cobro con ID {payment.CollectionId} no permite exoneraciones");
            }
            
            // Obtener el estudiante
            var student = await _studentRepository.GetByIdAsync(payment.StudentId);
            _logger.LogInformation("Service: Estudiante obtenido con ID {StudentId}", payment.StudentId);
            
            // Guardar las imágenes en la carpeta específica del pago
            if (dto.Images != null && dto.Images.Count > 0)
            {
                _logger.LogInformation("Service: Guardando {Count} imágenes para el pago exonerado", dto.Images.Count);
                var imagePaths = await _fileService.SaveImagesAsync(dto.Images, $"payments/{id}");
                payment.Images.AddRange(imagePaths);
            }
            
            // Actualizar el pago a exonerado
            payment.PaymentStatus = PaymentStatus.Exonerated;
            payment.AmountPaid = 0;
            payment.Pending = 0; // Asegurarnos de que el monto pendiente sea 0
            payment.Excedent = 0;
            payment.PaymentDate = dto.PaymentDate ?? DateTime.UtcNow;
            payment.Comment = $"PAGO EXONERADO: {dto.Comment}";
            payment.UpdatedAt = DateTime.UtcNow;
            
            await _paymentRepository.UpdateAsync(payment);
            _logger.LogInformation("Service: Pago actualizado como exonerado");
            
            // Actualizar el avance del gasto
            await UpdateCollectionAdvance(payment.CollectionId);
            
            // Usar el servicio de caja chica para crear una transacción de tipo Exonerated
            await _pettyCashService.RegisterExoneratedPaymentAsync(
                payment.Id,
                $"Pago exonerado para {collection?.Name} - Estudiante: {student?.Name ?? "Desconocido"} - {dto.Comment}"
            );
            
            _logger.LogInformation("Service: Transacción de exoneración registrada");
            
            stopwatch.Stop();
            _logger.LogInformation("Service: Exoneración de pago completada en {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
            
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
            using var activity = _activitySource.StartActivity("UpdateCollectionAdvance");
            activity?.SetTag("collection.id", collectionId);
            
            _logger.LogInformation("Actualizando avance de colección {CollectionId}", collectionId);

            try
            {
                var collection = await _collectionRepository.GetByIdAsync(collectionId);
                if (collection == null)
                {
                    _logger.LogWarning("Colección no encontrada con ID: {CollectionId}", collectionId);
                    return;
                }

                var payments = await _paymentRepository.GetByCollectionIdAsync(collectionId);
                
                var previousAdvance = new
                {
                    Total = collection.Advance.Total,
                    Completed = collection.Advance.Completed,
                    Pending = collection.Advance.Pending
                };

                collection.Advance.Total = payments.Count();
                collection.Advance.Completed = payments.Count(p => p.PaymentStatus == PaymentStatus.Paid);
                collection.Advance.Pending = collection.Advance.Total - collection.Advance.Completed;

                var totalPaid = payments.Sum(p => p.AmountPaid);
                decimal totalAmount = collection.AdjustedIndividualAmount.HasValue 
                    ? collection.AdjustedIndividualAmount.Value * collection.Advance.Total
                    : collection.IndividualAmount * collection.Advance.Total;

                collection.PercentagePaid = totalAmount > 0 ? (totalPaid / totalAmount) * 100 : 0;

                _logger.LogInformation(
                    "Avance actualizado para colección {CollectionId}. Anterior: {PreviousAdvance}, Nuevo: {NewAdvance}",
                    collectionId,
                    previousAdvance,
                    new { collection.Advance.Total, collection.Advance.Completed, collection.Advance.Pending }
                );

                await _collectionRepository.UpdateAsync(collection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar avance de colección {CollectionId}", collectionId);
                throw;
            }
        }

        // Método privado para enviar notificaciones de pago
        private async Task SendPaymentNotification(StudentPaymentDto payment, string studentName, string collectionName)
        {
            try
            {
                _logger.LogInformation("Enviando notificación de nuevo pago para {StudentName} en {CollectionName}", studentName, collectionName);
                
                // Datos adicionales para la notificación
                object notificationData;
                
                // Si hay imágenes, incluir la primera en los datos de la notificación
                if (payment.Images != null && payment.Images.Count > 0)
                {
                    notificationData = new 
                    {
                        PaymentId = payment.Id,
                        StudentId = payment.StudentId,
                        StudentName = studentName,
                        CollectionId = payment.CollectionId,
                        CollectionName = collectionName,
                        Amount = payment.AmountPaid,
                        Status = payment.PaymentStatus.ToString(),
                        ImageUrl = payment.Images[0]
                    };
                }
                else
                {
                    notificationData = new
                    {
                        PaymentId = payment.Id,
                        StudentId = payment.StudentId,
                        StudentName = studentName,
                        CollectionId = payment.CollectionId,
                        CollectionName = collectionName,
                        Amount = payment.AmountPaid,
                        Status = payment.PaymentStatus.ToString()
                    };
                }
                
                // Enviar notificación a todos
                var title = "Nuevo pago registrado";
                var body = $"Se ha registrado un pago de {studentName} para {collectionName}";
                
                await _notificationService.SendNotificationAsync("General", title, body, notificationData);
                
                _logger.LogInformation("Notificación de pago enviada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de pago: {Message}", ex.Message);
                // No propagar la excepción para no interrumpir el flujo principal
            }
        }
    }
} 