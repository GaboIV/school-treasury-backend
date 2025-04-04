using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPettyCashService
    {
        Task<PettyCashDto> GetPettyCashAsync();
        Task<TransactionDto> AddTransactionAsync(CreateTransactionDto transactionDto);
        Task<TransactionDto> RegisterExpenseFromPaymentAsync(string entityId);
        Task<TransactionDto> RegisterExpenseFromPaymentAsync(string entityId, decimal amount, string description);
        Task<TransactionDto> RegisterIncomeFromExcedentAsync(string paymentId, decimal amount, string description);
        Task<TransactionDto> RegisterExoneratedPaymentAsync(string paymentId, string description);
        Task<TransactionSummaryDto> GetSummaryAsync();
        Task<PaginatedTransactionDto> GetTransactionsAsync(int pageIndex = 0, int pageSize = 10);
        Task<bool> RecalculateBalancesInTransactionsAsync();
        Task<PettyCashCommentsDto> GetPettyCashCommentsAsync();
        Task<PettyCashCommentsDto> UpdatePettyCashCommentsAsync(PettyCashCommentsDto commentsDto);
    }
} 