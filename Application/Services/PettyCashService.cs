using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class PettyCashService : IPettyCashService
    {
        private readonly IPettyCashRepository _pettyCashRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IStudentPaymentRepository _studentPaymentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICollectionRepository _collectionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PettyCashService> _logger;

        public PettyCashService(
            IPettyCashRepository pettyCashRepository,
            ITransactionRepository transactionRepository,
            IStudentPaymentRepository studentPaymentRepository,
            IStudentRepository studentRepository,
            ICollectionRepository collectionRepository,
            IMapper mapper,
            ILogger<PettyCashService> logger)
        {
            _pettyCashRepository = pettyCashRepository;
            _transactionRepository = transactionRepository;
            _studentPaymentRepository = studentPaymentRepository;
            _studentRepository = studentRepository;
            _collectionRepository = collectionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PettyCashDto> GetPettyCashAsync()
        {
            var pettyCash = await _pettyCashRepository.GetAsync();
            if (pettyCash == null)
            {
                pettyCash = new PettyCash();
                await _pettyCashRepository.CreateAsync(pettyCash);
            }

            return _mapper.Map<PettyCashDto>(pettyCash);
        }

        public async Task<TransactionSummaryDto> GetSummaryAsync()
        {
            var pettyCash = await _pettyCashRepository.GetAsync();
            if (pettyCash == null)
            {
                pettyCash = new PettyCash();
                await _pettyCashRepository.CreateAsync(pettyCash);
            }

            var recentTransactions = await _transactionRepository.GetPaginatedAsync(1, 5);

            return new TransactionSummaryDto
            {
                Balance = pettyCash.CurrentBalance,
                TotalIncome = pettyCash.TotalIncome,
                TotalExpense = pettyCash.TotalExpense,
                LastTransactionDate = recentTransactions.Any() ? recentTransactions.First().Date : (DateTime?)null,
                RecentTransactions = _mapper.Map<List<TransactionDto>>(recentTransactions)
            };
        }

        public async Task<TransactionDto> AddTransactionAsync(CreateTransactionDto transactionDto)
        {
            var transaction = _mapper.Map<Transaction>(transactionDto);
            
            // Actualizar el balance en la caja chica
            await _pettyCashRepository.UpdateBalanceAsync(transaction.Amount, transaction.Type);
            
            // Guardar la transacción en la colección separada
            var addedTransaction = await _transactionRepository.CreateAsync(transaction);
            
            return _mapper.Map<TransactionDto>(addedTransaction);
        }

        public async Task<TransactionDto> RegisterExpenseFromPaymentAsync(string entityId)
        {
            try
            {
                // Intentar obtener el pago
                var payment = await _studentPaymentRepository.GetByIdAsync(entityId);
                
                // Si no es un pago, intentar obtener un gasto
                if (payment == null)
                {
                    var collectionEntity = await _collectionRepository.GetByIdAsync(entityId);
                    if (collectionEntity == null)
                    {
                        throw new KeyNotFoundException($"No se encontró ninguna entidad con el ID {entityId}");
                    }
                    
                    // Es un gasto
                    return await RegisterExpenseFromPaymentAsync(
                        entityId,
                        collectionEntity.TotalAmount,
                        $"Gasto: {collectionEntity.Name}"
                    );
                }
                
                // Obtener información del estudiante
                var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                
                // Obtener información del gasto
                var collection = await _collectionRepository.GetByIdAsync(payment.CollectionId);
                
                // Es un pago
                return await RegisterExpenseFromPaymentAsync(
                    entityId,
                    payment.AmountPaid,
                    $"Pago de estudiante: {student?.Name ?? "Desconocido"} - Gasto: {collection?.Name ?? "Desconocido"}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar gasto desde el pago {entityId}");
                throw;
            }
        }

        public async Task<TransactionDto> RegisterExpenseFromPaymentAsync(string entityId, decimal amount, string description)
        {
            try
            {
                // Obtener información adicional si es un pago
                string? studentId = null;
                string? studentName = null;
                string? collectionId = null;
                string? collectionName = null;
                string? paymentId = null;
                string? paymentStatus = null;
                
                var payment = await _studentPaymentRepository.GetByIdAsync(entityId);
                if (payment != null)
                {
                    paymentId = payment.Id;
                    paymentStatus = payment.PaymentStatus.ToString();
                    studentId = payment.StudentId;
                    
                    // Obtener información del estudiante
                    var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                    studentName = student?.Name;
                    
                    // Obtener información del gasto
                    collectionId = payment.CollectionId;
                    var collection = await _collectionRepository.GetByIdAsync(payment.CollectionId);
                    collectionName = collection?.Name;
                }
                
                var transaction = new Transaction
                {
                    Type = TransactionType.Collection,
                    Amount = amount,
                    Description = description,
                    RelatedEntityId = entityId,
                    RelatedEntityType = payment != null ? "Payment" : "Collection",
                    StudentId = studentId,
                    StudentName = studentName,
                    CollectionId = collectionId,
                    CollectionName = collectionName,
                    PaymentId = paymentId,
                    PaymentStatus = paymentStatus
                };

                // Actualizar el balance en la caja chica
                await _pettyCashRepository.UpdateBalanceAsync(amount, TransactionType.Collection);
                
                // Guardar la transacción en la colección separada
                var addedTransaction = await _transactionRepository.CreateAsync(transaction);
                
                return _mapper.Map<TransactionDto>(addedTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar gasto desde la entidad {entityId}");
                throw;
            }
        }

        public async Task<TransactionDto> RegisterIncomeFromExcedentAsync(string paymentId, decimal amount, string description)
        {
            try
            {
                // Obtener información del pago
                var payment = await _studentPaymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    throw new KeyNotFoundException($"No se encontró el pago con ID {paymentId}");
                }
                
                // Obtener información del estudiante
                var student = await _studentRepository.GetByIdAsync(payment.StudentId);
                
                // Obtener información del gasto
                var collection = await _collectionRepository.GetByIdAsync(payment.CollectionId);
                
                var transaction = new Transaction
                {
                    Type = TransactionType.Income,
                    Amount = amount,
                    Description = description,
                    RelatedEntityId = paymentId,
                    RelatedEntityType = "Payment",
                    StudentId = payment.StudentId,
                    StudentName = student?.Name,
                    CollectionId = payment.CollectionId,
                    CollectionName = collection?.Name,
                    PaymentId = payment.Id,
                    PaymentStatus = payment.PaymentStatus.ToString()
                };

                // Actualizar el balance en la caja chica
                await _pettyCashRepository.UpdateBalanceAsync(amount, TransactionType.Income);
                
                // Guardar la transacción en la colección separada
                var addedTransaction = await _transactionRepository.CreateAsync(transaction);
                
                return _mapper.Map<TransactionDto>(addedTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar ingreso desde el excedente {paymentId}");
                throw;
            }
        }

        public async Task<PaginatedTransactionDto> GetTransactionsAsync(int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                // Obtener las transacciones paginadas desde el repositorio de transacciones
                var transactions = await _transactionRepository.GetPaginatedAsync(pageIndex + 1, pageSize);
                var totalCount = await _transactionRepository.GetTotalCountAsync();
                
                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);
                
                return new PaginatedTransactionDto
                {
                    Items = transactionDtos,
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones paginadas");
                throw;
            }
        }
    }
} 