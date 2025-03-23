using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System.Linq;

namespace Application.Services
{
    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly IRepository<PaymentRequest> _paymentRequestRepository;
        private readonly IRepository<StudentPayment> _studentPaymentRepository;
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<Collection> _collectionRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICashboxService _cashboxService;
        private readonly IStudentPaymentService _studentPaymentService;
        private readonly IFileService _fileService;

        public PaymentRequestService(
            IRepository<PaymentRequest> paymentRequestRepository,
            IRepository<StudentPayment> studentPaymentRepository,
            IRepository<Student> studentRepository,
            IRepository<Collection> collectionRepository,
            IMapper mapper,
            IWebHostEnvironment webHostEnvironment,
            ICashboxService cashboxService,
            IStudentPaymentService studentPaymentService,
            IFileService fileService)
        {
            _paymentRequestRepository = paymentRequestRepository;
            _studentPaymentRepository = studentPaymentRepository;
            _studentRepository = studentRepository;
            _collectionRepository = collectionRepository;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _cashboxService = cashboxService;
            _studentPaymentService = studentPaymentService;
            _fileService = fileService;
        }

        public async Task<IEnumerable<PaymentRequestDto>> GetAllPaymentRequestsAsync()
        {
            var requests = await _paymentRequestRepository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<PaymentRequestDto>>(requests);
            
            return await EnrichPaymentRequestDtos(dtos);
        }

        public async Task<PaymentRequestDto> GetPaymentRequestByIdAsync(string id)
        {
            var request = await GetRequestById(id);
            var dto = _mapper.Map<PaymentRequestDto>(request);
            
            return await EnrichPaymentRequestDto(dto);
        }

        public async Task<IEnumerable<PaymentRequestDto>> GetPaymentRequestsByStudentIdAsync(string studentId)
        {
            var requests = await _paymentRequestRepository.FindAsync(r => r.StudentId == studentId);
            var dtos = _mapper.Map<IEnumerable<PaymentRequestDto>>(requests);
            
            return await EnrichPaymentRequestDtos(dtos);
        }

        public async Task<IEnumerable<PaymentRequestDto>> GetPaymentRequestsByStatusAsync(PaymentRequestStatus status)
        {
            var requests = await _paymentRequestRepository.FindAsync(r => r.Status == status);
            var dtos = _mapper.Map<IEnumerable<PaymentRequestDto>>(requests);
            
            return await EnrichPaymentRequestDtos(dtos);
        }

        public async Task<PaymentRequestDto> CreatePaymentRequestAsync(CreatePaymentRequestDto dto, string userId)
        {
            // Validar que el estudiante exista
            var student = await _studentRepository.GetByIdAsync(dto.StudentId);
            if (student == null)
                throw new KeyNotFoundException($"No se encontró el estudiante con ID {dto.StudentId}");

            // Validar que la colección exista
            var collection = await _collectionRepository.GetByIdAsync(dto.CollectionId);
            if (collection == null)
                throw new KeyNotFoundException($"No se encontró la colección con ID {dto.CollectionId}");

            // Buscar el pago del estudiante para esta colección
            var studentPayments = await _studentPaymentRepository.FindAsync(p => 
                p.StudentId == dto.StudentId && p.CollectionId == dto.CollectionId);
            
            var studentPayment = studentPayments.FirstOrDefault();
            
            decimal pendingAmount = 0;
            pendingAmount = collection.AdjustedIndividualAmount ?? collection.IndividualAmount;
            
            // Crear la solicitud de pago
            var paymentRequest = new PaymentRequest
            {
                CollectionId = dto.CollectionId,
                StudentId = dto.StudentId,
                AmountPaid = dto.AmountPaid,
                PendingAmount = pendingAmount,
                Comment = dto.Comment,
                PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
                Status = PaymentRequestStatus.Pending
            };

            // Agregar la entrada al historial
            var historyEntry = new PaymentRequestHistoryEntry
            {
                Action = "Solicitud creada",
                UserId = userId,
                UserRole = "Representante",
                Details = "Solicitud de pago creada por el representante",
                NewStatus = PaymentRequestStatus.Pending
            };
            paymentRequest.HistoryEntries.Add(historyEntry);

            await _paymentRequestRepository.AddAsync(paymentRequest);

            var result = _mapper.Map<PaymentRequestDto>(paymentRequest);
            return await EnrichPaymentRequestDto(result);
        }

        public async Task<PaymentRequestDto> CreatePaymentRequestWithImagesAsync(CreatePaymentRequestWithImagesDto dto, string userId)
        {
            // Validar que el estudiante exista
            var student = await _studentRepository.GetByIdAsync(dto.StudentId);
            if (student == null)
                throw new KeyNotFoundException($"No se encontró el estudiante con ID {dto.StudentId}");

            // Validar que la colección exista
            var collection = await _collectionRepository.GetByIdAsync(dto.CollectionId);
            if (collection == null)
                throw new KeyNotFoundException($"No se encontró la colección con ID {dto.CollectionId}");

            // Buscar el pago del estudiante para esta colección
            var studentPayments = await _studentPaymentRepository.FindAsync(p => 
                p.StudentId == dto.StudentId && p.CollectionId == dto.CollectionId);
            
            var studentPayment = studentPayments.FirstOrDefault();
            
            decimal pendingAmount = 0;

            pendingAmount = collection.AdjustedIndividualAmount ?? collection.IndividualAmount;

            // Crear la solicitud de pago
            var paymentRequest = new PaymentRequest
            {
                CollectionId = dto.CollectionId,
                StudentId = dto.StudentId,
                AmountPaid = dto.AmountPaid,
                PendingAmount = pendingAmount,
                Comment = dto.Comment,
                PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
                Status = PaymentRequestStatus.Pending
            };

            // Procesar imágenes
            if (dto.Images != null && dto.Images.Count > 0)
            {
                var uploadedImagePaths = await UploadImages(dto.Images);
                paymentRequest.Images.AddRange(uploadedImagePaths);
            }

            // Agregar la entrada al historial
            var historyEntry = new PaymentRequestHistoryEntry
            {
                Action = "Solicitud creada",
                UserId = userId,
                UserRole = "Representante",
                Details = "Solicitud de pago creada por el representante",
                NewStatus = PaymentRequestStatus.Pending
            };
            paymentRequest.HistoryEntries.Add(historyEntry);

            await _paymentRequestRepository.AddAsync(paymentRequest);

            var result = _mapper.Map<PaymentRequestDto>(paymentRequest);
            return await EnrichPaymentRequestDto(result);
        }

        public async Task<PaymentRequestDto> UpdatePaymentRequestAsync(string id, UpdatePaymentRequestDto dto, string userId)
        {
            var request = await GetRequestById(id);

            // Validar si la solicitud está en estado que permite actualización
            if (request.Status != PaymentRequestStatus.Pending && request.Status != PaymentRequestStatus.NeedsChanges)
                throw new InvalidOperationException($"No se puede actualizar la solicitud en estado {request.Status}");

            // Obtener el estado anterior para el historial
            var previousStatus = request.Status;

            // Actualizar los datos
            request.AmountPaid = dto.AmountPaid;
            request.Comment = dto.Comment;
            request.PaymentDate = dto.PaymentDate ?? request.PaymentDate;
            
            if (dto.Images != null)
                request.Images = dto.Images;
            
            if (dto.Voucher != null)
                request.Voucher = dto.Voucher;

            // Si estaba en estado "NeedsChanges", cambiar a "Pending"
            if (request.Status == PaymentRequestStatus.NeedsChanges)
                request.Status = PaymentRequestStatus.Pending;

            // Agregar la entrada al historial
            var historyEntry = new PaymentRequestHistoryEntry
            {
                Action = "Solicitud actualizada",
                UserId = userId,
                UserRole = "Representante",
                Details = "Solicitud de pago actualizada por el representante",
                PreviousStatus = previousStatus,
                NewStatus = request.Status
            };
            request.HistoryEntries.Add(historyEntry);

            await _paymentRequestRepository.UpdateAsync(request);

            var result = _mapper.Map<PaymentRequestDto>(request);
            return await EnrichPaymentRequestDto(result);
        }

        public async Task<PaymentRequestDto> UpdatePaymentRequestWithImagesAsync(string id, UpdatePaymentRequestWithImagesDto dto, string userId)
        {
            var request = await GetRequestById(id);

            // Validar si la solicitud está en estado que permite actualización
            if (request.Status != PaymentRequestStatus.Pending && request.Status != PaymentRequestStatus.NeedsChanges)
                throw new InvalidOperationException($"No se puede actualizar la solicitud en estado {request.Status}");

            // Obtener el estado anterior para el historial
            var previousStatus = request.Status;

            // Actualizar los datos
            request.AmountPaid = dto.AmountPaid;
            request.Comment = dto.Comment;
            request.PaymentDate = dto.PaymentDate ?? request.PaymentDate;

            // Procesar nuevas imágenes
            if (dto.Images != null && dto.Images.Count > 0)
            {
                // Eliminar imágenes anteriores
                request.Images.Clear();
                
                // Subir nuevas imágenes
                var uploadedImagePaths = await UploadImages(dto.Images);
                request.Images.AddRange(uploadedImagePaths);
            }

            // Si estaba en estado "NeedsChanges", cambiar a "Pending"
            if (request.Status == PaymentRequestStatus.NeedsChanges)
                request.Status = PaymentRequestStatus.Pending;

            // Agregar la entrada al historial
            var historyEntry = new PaymentRequestHistoryEntry
            {
                Action = "Solicitud actualizada",
                UserId = userId,
                UserRole = "Representante",
                Details = "Solicitud de pago actualizada por el representante con nuevas imágenes",
                PreviousStatus = previousStatus,
                NewStatus = request.Status
            };
            request.HistoryEntries.Add(historyEntry);

            await _paymentRequestRepository.UpdateAsync(request);

            var result = _mapper.Map<PaymentRequestDto>(request);
            return await EnrichPaymentRequestDto(result);
        }

        public async Task DeletePaymentRequestAsync(string id)
        {
            var request = await GetRequestById(id);

            // Validar si la solicitud está en estado que permite eliminación
            if (request.Status != PaymentRequestStatus.Pending && request.Status != PaymentRequestStatus.Rejected)
                throw new InvalidOperationException($"No se puede eliminar la solicitud en estado {request.Status}");

            await _paymentRequestRepository.DeleteAsync(id);
        }

        public async Task<PaymentRequestDto> ApprovePaymentRequestAsync(string id, ApprovePaymentRequestDto dto)
        {
            var request = await GetRequestById(id);

            // Validar si la solicitud está en estado que permite aprobación
            if (request.Status != PaymentRequestStatus.Pending && request.Status != PaymentRequestStatus.UnderReview)
                throw new InvalidOperationException($"No se puede aprobar la solicitud en estado {request.Status}");

            // Obtener el estado anterior para el historial
            var previousStatus = request.Status;

            // Actualizar el estado
            request.Status = PaymentRequestStatus.Approved;
            request.ApprovedByAdminId = dto.AdminId;
            request.ApprovedAt = DateTime.UtcNow;

            // Agregar comentario del administrador si existe
            if (!string.IsNullOrEmpty(dto.AdminComment))
            {
                var adminComment = new AdminComment
                {
                    AdminId = dto.AdminId,
                    Comment = dto.AdminComment,
                    IsInternal = false
                };
                request.AdminComments.Add(adminComment);
            }

            // Agregar la entrada al historial
            var historyEntry = new PaymentRequestHistoryEntry
            {
                Action = "Solicitud aprobada",
                UserId = dto.AdminId,
                UserRole = "Administrador",
                Details = "Solicitud de pago aprobada por el administrador",
                PreviousStatus = previousStatus,
                NewStatus = PaymentRequestStatus.Approved
            };
            request.HistoryEntries.Add(historyEntry);

            // Crear el pago de estudiante basado en la solicitud
            var createPaymentDto = new CreateStudentPaymentDto
            {
                CollectionId = request.CollectionId,
                StudentId = request.StudentId,
                AmountPaid = request.AmountPaid,
                Images = request.Images,
                Voucher = request.Voucher,
                Comment = request.Comment,
                PaymentDate = request.PaymentDate
            };

            var payment = await _studentPaymentService.CreatePaymentAsync(createPaymentDto);
            
            // Registrar el ID del pago creado en la solicitud
            request.StudentPaymentId = payment.Id;

            // Actualizar el registro de la solicitud
            await _paymentRequestRepository.UpdateAsync(request);

            // Registrar el movimiento en la caja chica
            await _cashboxService.RegisterIncomeAsync(new RegisterCashboxMovementDto
            {
                Amount = request.AmountPaid,
                Concept = $"Pago aprobado del estudiante {payment.StudentName} para la colección {payment.CollectionName}",
                SourceId = payment.Id,
                SourceType = "StudentPayment"
            });

            var result = _mapper.Map<PaymentRequestDto>(request);
            return await EnrichPaymentRequestDto(result);
        }

        public async Task<PaymentRequestDto> RejectPaymentRequestAsync(string id, RejectPaymentRequestDto dto)
        {
            var request = await GetRequestById(id);

            // Validar si la solicitud está en estado que permite rechazo
            if (request.Status != PaymentRequestStatus.Pending && request.Status != PaymentRequestStatus.UnderReview)
                throw new InvalidOperationException($"No se puede rechazar la solicitud en estado {request.Status}");

            // Obtener el estado anterior para el historial
            var previousStatus = request.Status;

            // Actualizar el estado
            request.Status = PaymentRequestStatus.Rejected;
            request.RejectionReason = dto.RejectionReason;

            // Agregar la entrada al historial
            var historyEntry = new PaymentRequestHistoryEntry
            {
                Action = "Solicitud rechazada",
                UserId = dto.AdminId,
                UserRole = "Administrador",
                Details = $"Solicitud de pago rechazada. Motivo: {dto.RejectionReason}",
                PreviousStatus = previousStatus,
                NewStatus = PaymentRequestStatus.Rejected
            };
            request.HistoryEntries.Add(historyEntry);

            await _paymentRequestRepository.UpdateAsync(request);

            var result = _mapper.Map<PaymentRequestDto>(request);
            return await EnrichPaymentRequestDto(result);
        }

        public async Task<PaymentRequestDto> RequestChangesAsync(string id, RequestChangesDto dto)
        {
            var request = await GetRequestById(id);

            // Validar si la solicitud está en estado que permite solicitar cambios
            if (request.Status != PaymentRequestStatus.Pending && request.Status != PaymentRequestStatus.UnderReview)
                throw new InvalidOperationException($"No se puede solicitar cambios para la solicitud en estado {request.Status}");

            // Obtener el estado anterior para el historial
            var previousStatus = request.Status;

            // Actualizar el estado
            request.Status = PaymentRequestStatus.NeedsChanges;

            // Agregar comentario del administrador
            var adminComment = new AdminComment
            {
                AdminId = dto.AdminId,
                AdminName = dto.AdminName,
                Comment = dto.Comment,
                IsInternal = dto.IsInternal
            };
            request.AdminComments.Add(adminComment);

            // Agregar la entrada al historial
            var historyEntry = new PaymentRequestHistoryEntry
            {
                Action = "Cambios solicitados",
                UserId = dto.AdminId,
                UserRole = "Administrador",
                Details = "Se han solicitado cambios para la solicitud de pago",
                PreviousStatus = previousStatus,
                NewStatus = PaymentRequestStatus.NeedsChanges
            };
            request.HistoryEntries.Add(historyEntry);

            await _paymentRequestRepository.UpdateAsync(request);

            var result = _mapper.Map<PaymentRequestDto>(request);
            return await EnrichPaymentRequestDto(result);
        }

        public async Task<PaymentRequestDto> AddAdminCommentAsync(string id, AddAdminCommentDto dto)
        {
            var request = await GetRequestById(id);

            // Agregar comentario del administrador
            var adminComment = new AdminComment
            {
                AdminId = dto.AdminId,
                AdminName = dto.AdminName,
                Comment = dto.Comment,
                IsInternal = dto.IsInternal
            };
            request.AdminComments.Add(adminComment);

            // Agregar la entrada al historial
            var historyEntry = new PaymentRequestHistoryEntry
            {
                Action = "Comentario agregado",
                UserId = dto.AdminId,
                UserRole = "Administrador",
                Details = dto.IsInternal ? "Comentario interno agregado" : "Comentario agregado para el representante",
            };
            request.HistoryEntries.Add(historyEntry);

            await _paymentRequestRepository.UpdateAsync(request);

            var result = _mapper.Map<PaymentRequestDto>(request);
            return await EnrichPaymentRequestDto(result);
        }

        public async Task<PaymentRequestDto> ChangeStatusAsync(string id, PaymentRequestStatus newStatus, string userId, string userRole, string details)
        {
            var request = await GetRequestById(id);

            // Obtener el estado anterior para el historial
            var previousStatus = request.Status;

            // Actualizar el estado
            request.Status = newStatus;

            // Agregar la entrada al historial
            var historyEntry = new PaymentRequestHistoryEntry
            {
                Action = $"Cambio de estado a {newStatus}",
                UserId = userId,
                UserRole = userRole,
                Details = details,
                PreviousStatus = previousStatus,
                NewStatus = newStatus
            };
            request.HistoryEntries.Add(historyEntry);

            await _paymentRequestRepository.UpdateAsync(request);

            var result = _mapper.Map<PaymentRequestDto>(request);
            return await EnrichPaymentRequestDto(result);
        }

        public async Task<IEnumerable<PaymentRequestDto>> GetPendingRequestsAsync()
        {
            return await GetPaymentRequestsByStatusAsync(PaymentRequestStatus.Pending);
        }

        public async Task<IEnumerable<PaymentRequestDto>> GetUnderReviewRequestsAsync()
        {
            return await GetPaymentRequestsByStatusAsync(PaymentRequestStatus.UnderReview);
        }

        public async Task<IEnumerable<PaymentRequestDto>> GetNeedsChangesRequestsAsync()
        {
            return await GetPaymentRequestsByStatusAsync(PaymentRequestStatus.NeedsChanges);
        }

        public async Task<IEnumerable<PaymentRequestDto>> GetRequestHistoryByStudentIdAsync(string studentId)
        {
            var requests = await _paymentRequestRepository.FindAsync(r => r.StudentId == studentId);
            var sortedRequests = requests.OrderByDescending(r => r.CreatedAt);
            var dtos = _mapper.Map<IEnumerable<PaymentRequestDto>>(sortedRequests);
            
            return await EnrichPaymentRequestDtos(dtos);
        }

        private async Task<PaymentRequest> GetRequestById(string id)
        {
            var request = await _paymentRequestRepository.GetByIdAsync(id);
            if (request == null)
                throw new KeyNotFoundException($"No se encontró la solicitud de pago con ID {id}");
                
            return request;
        }

        private async Task<IEnumerable<PaymentRequestDto>> EnrichPaymentRequestDtos(IEnumerable<PaymentRequestDto> dtos)
        {
            foreach (var dto in dtos)
            {
                await EnrichPaymentRequestDto(dto);
            }
            return dtos;
        }

        private async Task<PaymentRequestDto> EnrichPaymentRequestDto(PaymentRequestDto dto)
        {
            // Agregar información del estudiante
            var student = await _studentRepository.GetByIdAsync(dto.StudentId);
            if (student != null)
            {
                dto.StudentName = $"{student.Name}";
            }

            // Agregar información de la colección
            var collection = await _collectionRepository.GetByIdAsync(dto.CollectionId);
            if (collection != null)
            {
                dto.CollectionName = collection.Name;
                dto.AmountCollection = collection.TotalAmount;
                dto.Collection = _mapper.Map<CollectionDto>(collection);
            }

            // Si está aprobada y tiene ID de pago asociado, obtener información del administrador
            if (dto.Status == PaymentRequestStatus.Approved && !string.IsNullOrEmpty(dto.ApprovedByAdminId))
            {
                // Aquí se podría obtener el nombre del administrador desde un servicio de usuarios
                dto.ApprovedByAdminName = "Administrador"; // Valor por defecto
            }

            // Convertir rutas relativas de imágenes a URLs absolutas
            dto.Images = ConvertToAbsoluteUrls(dto.Images);

            return dto;
        }

        private List<string> ConvertToAbsoluteUrls(List<string> relativeUrls)
        {
            var absoluteUrls = new List<string>();
            
            foreach (var relativeUrl in relativeUrls)
            {
                if (!string.IsNullOrEmpty(relativeUrl))
                {
                    // Usar el FileService para convertir la ruta relativa a URL absoluta
                    absoluteUrls.Add(_fileService.GetImageUrl(relativeUrl));
                }
            }
            
            return absoluteUrls;
        }

        private async Task<List<string>> UploadImages(List<IFormFile> images)
        {
            var uploadedPaths = new List<string>();
            
            if (images == null || images.Count == 0)
                return uploadedPaths;
                
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "payment-requests");
            
            // Crear el directorio si no existe
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
                
            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    // Generar un nombre único para la imagen
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    // Guardar la imagen en el servidor
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    
                    // Agregar la ruta relativa a la lista de rutas
                    uploadedPaths.Add($"/uploads/payment-requests/{fileName}");
                }
            }
            
            return uploadedPaths;
        }
    }
} 