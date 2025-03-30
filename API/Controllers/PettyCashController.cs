using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/petty-cash")]
    public class PettyCashController : ControllerBase
    {
        private readonly IPettyCashService _pettyCashService;

        public PettyCashController(IPettyCashService pettyCashService)
        {
            _pettyCashService = pettyCashService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PettyCashDto>>> GetPettyCash()
        {
            try
            {
                var pettyCash = await _pettyCashService.GetPettyCashAsync();
                return Ok(new ApiResponse<PettyCashDto>(
                    pettyCash,
                    "Caja chica obtenida correctamente",
                    true
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PettyCashDto>(
                    null,
                    $"Error al obtener la caja chica: {ex.Message}",
                    false
                ));
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<TransactionSummaryDto>>> GetSummary()
        {
            try
            {
                var summary = await _pettyCashService.GetSummaryAsync();
                return Ok(new ApiResponse<TransactionSummaryDto>(
                    summary,
                    "Resumen de transacciones obtenido correctamente",
                    true
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<TransactionSummaryDto>(
                    null,
                    $"Error al obtener el resumen de transacciones: {ex.Message}",
                    false
                ));
            }
        }

        [HttpPost("transaction")]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> AddTransaction([FromBody] CreateTransactionDto transactionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<TransactionDto>(
                    null,
                    "Datos de transacción inválidos",
                    false
                ));
            }

            try
            {
                var transaction = await _pettyCashService.AddTransactionAsync(transactionDto);
                return Ok(new ApiResponse<TransactionDto>(
                    transaction,
                    "Transacción agregada correctamente",
                    true
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<TransactionDto>(
                    null,
                    $"Error al agregar la transacción: {ex.Message}",
                    false
                ));
            }
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<ApiResponse<PaginatedTransactionDto>>> GetTransactions([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
        {
            try
            {
                var transactions = await _pettyCashService.GetTransactionsAsync(pageIndex, pageSize);
                return Ok(new ApiResponse<PaginatedTransactionDto>(
                    transactions,
                    "Transacciones obtenidas correctamente",
                    true
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PaginatedTransactionDto>(
                    null,
                    $"Error al obtener las transacciones: {ex.Message}",
                    false
                ));
            }
        }
        
        [HttpPost("recalculate-balances")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ApiResponse<bool>>> RecalculateBalances()
        {
            try
            {
                var result = await _pettyCashService.RecalculateBalancesInTransactionsAsync();
                
                if (result)
                {
                    return Ok(new ApiResponse<bool>(
                        true,
                        "Saldos de transacciones recalculados correctamente",
                        true
                    ));
                }
                else
                {
                    return StatusCode(500, new ApiResponse<bool>(
                        false,
                        "Error al recalcular los saldos de transacciones",
                        false
                    ));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>(
                    false,
                    $"Error al recalcular los saldos: {ex.Message}",
                    false
                ));
            }
        }
    }
} 