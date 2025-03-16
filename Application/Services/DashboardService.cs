using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IStudentPaymentRepository _studentPaymentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IPettyCashRepository _pettyCashRepository;
        private readonly IInterestLinkService _interestLinkService;
        private readonly IMapper _mapper;
        private readonly ICollectionRepository _collectionRepository;
        private readonly ICollectionTypeRepository _collectionTypeRepository;

        public DashboardService(
            IStudentPaymentRepository studentPaymentRepository,
            IStudentRepository studentRepository,
            IPettyCashRepository pettyCashRepository,
            IInterestLinkService interestLinkService,
            IMapper mapper,
            ICollectionRepository collectionRepository,
            ICollectionTypeRepository collectionTypeRepository)
        {
            _studentPaymentRepository = studentPaymentRepository ?? throw new ArgumentNullException(nameof(studentPaymentRepository));
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _pettyCashRepository = pettyCashRepository ?? throw new ArgumentNullException(nameof(pettyCashRepository));
            _interestLinkService = interestLinkService ?? throw new ArgumentNullException(nameof(interestLinkService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
            _collectionTypeRepository = collectionTypeRepository ?? throw new ArgumentNullException(nameof(collectionTypeRepository));
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var pendingPayments = await GetPendingPaymentsAsync();
            var studentsInfo = await GetStudentsInfoAsync();
            var pettyCashSummary = await GetPettyCashSummaryAsync();
            var interestLinks = await _interestLinkService.GetAllAsync();
            var topPendingCollections = await GetTopPendingCollectionsAsync();

            return new DashboardDto
            {
                PendingPayments = pendingPayments,
                StudentsInfo = studentsInfo,
                PettyCashSummary = pettyCashSummary,
                InterestLinks = interestLinks.ToList(),
                TopPendingCollections = topPendingCollections
            };
        }

        public async Task<PendingPaymentsDto> GetPendingPaymentsAsync()
        {
            var allPayments = await _studentPaymentRepository.GetAllAsync();
            var totalPayments = allPayments.Count();
            var pendingPayments = allPayments.Where(p => p.PaymentStatus == PaymentStatus.Pending || p.PaymentStatus == PaymentStatus.PartiallyPaid).ToList();
            var pendingPaymentsCount = pendingPayments.Count();
            
            decimal completionPercentage = 0;
            if (totalPayments > 0)
            {
                completionPercentage = Math.Round(((decimal)(totalPayments - pendingPaymentsCount) / totalPayments) * 100, 2);
            }

            // Calcular el monto total pendiente
            decimal totalPendingAmount = pendingPayments.Sum(p => p.Pending);

            // Obtener los 3 pagos pendientes principales
            var topPendingPayments = new List<PendingPaymentDetailDto>();
            int remainingPendingPayments = 0;

            if (pendingPayments.Any())
            {
                // Obtener todos los estudiantes y colecciones
                var allStudents = await _studentRepository.GetAllAsync();
                var allCollections = await _collectionRepository.GetAllAsync();
                
                // Crear diccionarios para búsqueda rápida
                var studentDict = allStudents.ToDictionary(s => s.Id, s => s);
                var collectionDict = allCollections.ToDictionary(c => c.Id, c => c);

                // Ordenar por monto pendiente (de mayor a menor)
                var orderedPendingPayments = pendingPayments.OrderByDescending(p => p.Pending).ToList();
                
                // Tomar los 3 primeros para el detalle
                var topPayments = orderedPendingPayments.Take(3).ToList();
                
                foreach (var payment in topPayments)
                {
                    studentDict.TryGetValue(payment.StudentId, out var student);
                    collectionDict.TryGetValue(payment.CollectionId, out var collection);
                    
                    topPendingPayments.Add(new PendingPaymentDetailDto
                    {
                        Id = payment.Id,
                        StudentId = payment.StudentId,
                        StudentName = student?.Name ?? "Estudiante desconocido",
                        CollectionName = collection?.Name ?? "Cobro desconocido",
                        PendingAmount = payment.Pending,
                        PaymentStatus = payment.PaymentStatus
                    });
                }
                
                // Calcular cuántos pagos pendientes quedan después de los 3 principales
                remainingPendingPayments = pendingPaymentsCount - topPayments.Count;
            }

            return new PendingPaymentsDto
            {
                TotalPendingPayments = pendingPaymentsCount,
                TotalPayments = totalPayments,
                CompletionPercentage = completionPercentage,
                TotalPendingAmount = totalPendingAmount,
                TopPendingPayments = topPendingPayments,
                RemainingPendingPayments = remainingPendingPayments
            };
        }

        public async Task<StudentsInfoDto> GetStudentsInfoAsync()
        {
            var students = await _studentRepository.GetAllAsync();
            var totalStudents = students.Count();

            var studentInitials = students.Select(s => new StudentInitialDto
            {
                Id = s.Id,
                Name = s.Name,
                Initial = !string.IsNullOrEmpty(s.Name) ? s.Name.Substring(0, 1).ToUpper() : ""
            }).ToList();

            return new StudentsInfoDto
            {
                TotalStudents = totalStudents,
                StudentInitials = studentInitials
            };
        }

        public async Task<PettyCashSummaryDto> GetPettyCashSummaryAsync()
        {
            var pettyCash = await _pettyCashRepository.GetAsync();
            
            if (pettyCash == null)
            {
                return new PettyCashSummaryDto();
            }

            // Calcular el porcentaje de cambio (ejemplo: comparando con el día anterior)
            decimal percentageChange = 0;
            // Aquí podrías implementar la lógica para calcular el porcentaje de cambio
            // Por ejemplo, comparando con el saldo del día anterior

            return new PettyCashSummaryDto
            {
                CurrentBalance = pettyCash.CurrentBalance,
                TotalIncome = pettyCash.TotalIncome,
                TotalExpense = pettyCash.TotalExpense,
                Available = pettyCash.CurrentBalance,
                PercentageChange = percentageChange
            };
        }

        public async Task<TopPendingCollectionsDto> GetTopPendingCollectionsAsync()
        {
            // Obtener todas las colecciones
            var allCollections = await _collectionRepository.GetAllAsync();
            
            // Filtrar las colecciones que tienen pagos pendientes (PercentagePaid < 100)
            var pendingCollections = allCollections
                .Where(c => c.PercentagePaid < 100)
                .ToList();
            
            var result = new TopPendingCollectionsDto();
            
            if (pendingCollections.Any())
            {
                // Ordenar por porcentaje de pago (ascendente) para obtener las menos completadas primero
                var orderedCollections = pendingCollections
                    .OrderBy(c => c.PercentagePaid)
                    .ToList();
                
                // Tomar los 3 primeros para el detalle
                var topCollections = orderedCollections.Take(3).ToList();
                
                // Obtener todos los tipos de colección
                var allCollectionTypes = await _collectionTypeRepository.GetAllAsync();
                
                // Crear un diccionario para búsqueda rápida
                var collectionTypeDict = allCollectionTypes.ToDictionary(t => t.Id ?? string.Empty, t => t.Name);
                
                foreach (var collection in topCollections)
                {
                    // Calcular el monto pendiente
                    decimal pendingAmount = collection.TotalAmount * (1 - (collection.PercentagePaid / 100));
                    
                    // Obtener el nombre del tipo de colección
                    string collectionTypeName = "Tipo desconocido";
                    if (!string.IsNullOrEmpty(collection.CollectionTypeId) && collectionTypeDict.ContainsKey(collection.CollectionTypeId))
                    {
                        collectionTypeName = collectionTypeDict[collection.CollectionTypeId];
                    }
                    
                    result.TopCollections.Add(new PendingCollectionDetailDto
                    {
                        Id = collection.Id,
                        Name = collection.Name ?? "Cobro sin nombre",
                        CollectionTypeName = collectionTypeName,
                        TotalAmount = collection.TotalAmount,
                        PendingAmount = pendingAmount,
                        TotalStudents = collection.Advance.Total,
                        PendingStudents = collection.Advance.Pending,
                        CompletionPercentage = collection.PercentagePaid
                    });
                }
                
                // Calcular cuántas colecciones pendientes quedan después de las 3 principales
                result.RemainingPendingCollections = pendingCollections.Count - topCollections.Count;
            }
            
            return result;
        }
    }
} 