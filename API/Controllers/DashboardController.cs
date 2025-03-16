using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILoggerManager _logger;

        public DashboardController(IDashboardService dashboardService, ILoggerManager logger)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> GetDashboardData()
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener datos del dashboard: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>("", "Error al obtener datos del dashboard", false));
            }
        }

        [HttpGet("pending-payments")]
        public async Task<ActionResult<PendingPaymentsDto>> GetPendingPayments()
        {
            try
            {
                var pendingPayments = await _dashboardService.GetPendingPaymentsAsync();
                return Ok(pendingPayments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener pagos pendientes: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>("", "Error al obtener pagos pendientes", false));
            }
        }

        [HttpGet("students-info")]
        public async Task<ActionResult<StudentsInfoDto>> GetStudentsInfo()
        {
            try
            {
                var studentsInfo = await _dashboardService.GetStudentsInfoAsync();
                return Ok(studentsInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener información de estudiantes: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>("", "Error al obtener información de estudiantes", false));
            }
        }

        [HttpGet("petty-cash-summary")]
        public async Task<ActionResult<PettyCashSummaryDto>> GetPettyCashSummary()
        {
            try
            {
                var pettyCashSummary = await _dashboardService.GetPettyCashSummaryAsync();
                return Ok(pettyCashSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener resumen de caja chica: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>("", "Error al obtener resumen de caja chica", false));
            }
        }
    }
}
