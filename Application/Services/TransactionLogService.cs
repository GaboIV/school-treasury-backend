using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Application.Services
{
    public class TransactionLogService : ITransactionLogService
    {
        private readonly ITransactionLogRepository _transactionLogRepository;
        private readonly IMapper _mapper;

        public TransactionLogService(
            ITransactionLogRepository transactionLogRepository,
            IMapper mapper)
        {
            _transactionLogRepository = transactionLogRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TransactionLogDto>> GetAllLogsAsync()
        {
            var logs = await _transactionLogRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TransactionLogDto>>(logs);
        }

        public async Task<TransactionLogDto> GetLogByIdAsync(string id)
        {
            var log = await _transactionLogRepository.GetByIdAsync(id);
            if (log == null)
            {
                throw new KeyNotFoundException($"Log de transacción con ID {id} no encontrado");
            }
            
            return _mapper.Map<TransactionLogDto>(log);
        }

        public async Task<IEnumerable<TransactionLogDto>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await _transactionLogRepository.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<TransactionLogDto>>(logs);
        }

        public async Task<IEnumerable<TransactionLogDto>> GetLogsByRelatedEntityAsync(string relatedEntityId, string relatedEntityType)
        {
            var logs = await _transactionLogRepository.GetByRelatedEntityAsync(relatedEntityId, relatedEntityType);
            return _mapper.Map<IEnumerable<TransactionLogDto>>(logs);
        }

        public async Task<TransactionLogDto> LogTransactionAsync(Transaction transaction, decimal balanceBefore, decimal balanceAfter, string userId = null, string userName = null, string ipAddress = null)
        {
            var log = new TransactionLog
            {
                TransactionId = transaction.Id,
                Date = transaction.Date,
                Type = transaction.Type,
                Amount = transaction.Amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                Description = transaction.Description,
                RelatedEntityId = transaction.RelatedEntityId,
                RelatedEntityType = transaction.RelatedEntityType,
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress
            };
            
            var addedLog = await _transactionLogRepository.AddAsync(log);
            return _mapper.Map<TransactionLogDto>(addedLog);
        }

        public async Task<IEnumerable<TransactionTimelineDto>> GetTimelineAsync(int count = 20)
        {
            var (logs, _) = await _transactionLogRepository.GetPaginatedAsync(1, count);
            
            return logs.Select(log => new TransactionTimelineDto
            {
                Id = log.Id,
                Date = log.Date,
                Title = GetTransactionTitle(log),
                Description = log.Description,
                Type = log.Type,
                Amount = log.Amount,
                Balance = log.BalanceAfter,
                RelatedEntityId = log.RelatedEntityId,
                RelatedEntityType = log.RelatedEntityType
            }).ToList();
        }

        public async Task<PaginatedResponseDto<IEnumerable<TransactionLogDto>>> GetPaginatedLogsAsync(int page, int pageSize)
        {
            var (logs, totalCount) = await _transactionLogRepository.GetPaginatedAsync(page, pageSize);
            
            var logDtos = _mapper.Map<IEnumerable<TransactionLogDto>>(logs);
            
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var paginationInfo = new PaginationDto
            {
                TotalItems = totalCount,
                ItemsPerPage = pageSize,
                CurrentPage = page,
                TotalPages = totalPages
            };
            
            return new PaginatedResponseDto<IEnumerable<TransactionLogDto>>(logDtos, paginationInfo);
        }

        private string GetTransactionTitle(TransactionLog log)
        {
            string typeText = log.Type == TransactionType.Income ? "Ingreso" : "Egreso";
            
            if (string.IsNullOrEmpty(log.RelatedEntityType))
            {
                return $"{typeText} de Caja Chica";
            }
            
            switch (log.RelatedEntityType)
            {
                case "StudentPayment":
                    return log.Type == TransactionType.Income 
                        ? "Ingreso por excedente de pago"
                        : "Egreso por registro de pago";
                default:
                    return $"{typeText} relacionado con {log.RelatedEntityType}";
            }
        }
    }
} 