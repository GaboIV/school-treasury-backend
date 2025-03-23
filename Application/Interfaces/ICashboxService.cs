using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICashboxService
    {
        Task<decimal> GetCurrentBalanceAsync();
        Task RegisterIncomeAsync(RegisterCashboxMovementDto dto);
        Task RegisterExpenseAsync(RegisterCashboxMovementDto dto);
    }
} 