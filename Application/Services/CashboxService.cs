using Application.DTOs;
using Application.Interfaces;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CashboxService : ICashboxService
    {
        private readonly IPettyCashService _pettyCashService;

        public CashboxService(IPettyCashService pettyCashService)
        {
            _pettyCashService = pettyCashService;
        }

        public async Task<decimal> GetCurrentBalanceAsync()
        {
            var pettyCash = await _pettyCashService.GetPettyCashAsync();
            return pettyCash?.Balance ?? 0;
        }

        public async Task RegisterIncomeAsync(RegisterCashboxMovementDto dto)
        {
            await _pettyCashService.RegisterIncomeFromExcedentAsync(
                dto.SourceId,
                dto.Amount,
                dto.Concept
            );
        }

        public async Task RegisterExpenseAsync(RegisterCashboxMovementDto dto)
        {
            await _pettyCashService.RegisterExpenseFromPaymentAsync(
                dto.SourceId,
                dto.Amount,
                dto.Concept
            );
        }
    }
} 