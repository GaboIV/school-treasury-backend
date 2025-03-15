using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/transaction-logs")]
    public class TransactionLogController : ControllerBase
    {
        private readonly ITransactionLogService _transactionLogService;

        public TransactionLogController(ITransactionLogService transactionLogService)
        {
            _transactionLogService = transactionLogService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionLogDto>>>> GetAllLogs()
        {
            try
            {
                var logs = await _transactionLogService.GetAllLogsAsync();
                var response = new ApiResponse<IEnumerable<TransactionLogDto>>(logs, "Logs obtenidos correctamente");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>($"Error al obtener los logs: {ex.Message}", "Error", false));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TransactionLogDto>>> GetLogById(string id)
        {
            try
            {
                var log = await _transactionLogService.GetLogByIdAsync(id);
                var response = new ApiResponse<TransactionLogDto>(log, "Log obtenido correctamente");
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(ex.Message, "Not Found", false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>($"Error al obtener el log: {ex.Message}", "Error", false));
            }
        }

        [HttpGet("timeline")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionTimelineDto>>>> GetTimeline([FromQuery] int count = 20)
        {
            try
            {
                var timeline = await _transactionLogService.GetTimelineAsync(count);
                var response = new ApiResponse<IEnumerable<TransactionTimelineDto>>(timeline, "Timeline obtenida correctamente");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>($"Error al obtener la timeline: {ex.Message}", "Error", false));
            }
        }

        [HttpGet("paginated")]
        public async Task<ActionResult<ApiResponse<PaginatedResponseDto<IEnumerable<TransactionLogDto>>>>> GetPaginatedLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;

                var paginatedLogs = await _transactionLogService.GetPaginatedLogsAsync(page, pageSize);
                var response = new ApiResponse<PaginatedResponseDto<IEnumerable<TransactionLogDto>>>(paginatedLogs, "Logs paginados obtenidos correctamente");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>($"Error al obtener los logs paginados: {ex.Message}", "Error", false));
            }
        }

        [HttpGet("by-date")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionLogDto>>>> GetLogsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var logs = await _transactionLogService.GetLogsByDateRangeAsync(startDate, endDate);
                var response = new ApiResponse<IEnumerable<TransactionLogDto>>(logs, "Logs por rango de fecha obtenidos correctamente");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>($"Error al obtener los logs por rango de fecha: {ex.Message}", "Error", false));
            }
        }

        [HttpGet("by-entity")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionLogDto>>>> GetLogsByRelatedEntity([FromQuery] string entityId, [FromQuery] string entityType)
        {
            try
            {
                var logs = await _transactionLogService.GetLogsByRelatedEntityAsync(entityId, entityType);
                var response = new ApiResponse<IEnumerable<TransactionLogDto>>(logs, "Logs por entidad relacionada obtenidos correctamente");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>($"Error al obtener los logs por entidad relacionada: {ex.Message}", "Error", false));
            }
        }
    }
} 