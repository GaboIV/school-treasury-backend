using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentPaymentController : ControllerBase
    {
        private readonly IStudentPaymentService _paymentService;

        public StudentPaymentController(IStudentPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<StudentPaymentDto>>>> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(new ApiResponse<IEnumerable<StudentPaymentDto>>(payments, "Pagos obtenidos exitosamente"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<StudentPaymentDto>>> GetPaymentById(string id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                return Ok(new ApiResponse<StudentPaymentDto>(payment, "Pago obtenido exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<StudentPaymentDto>(null, ex.Message, false));
            }
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StudentPaymentDto>>>> GetPaymentsByStudentId(string studentId)
        {
            var payments = await _paymentService.GetPaymentsByStudentIdAsync(studentId);
            return Ok(new ApiResponse<IEnumerable<StudentPaymentDto>>(payments, "Pagos del estudiante obtenidos exitosamente"));
        }

        [HttpGet("expense/{expenseId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StudentPaymentDto>>>> GetPaymentsByExpenseId(string expenseId)
        {
            var payments = await _paymentService.GetPaymentsByExpenseIdAsync(expenseId);
            return Ok(new ApiResponse<IEnumerable<StudentPaymentDto>>(payments, "Pagos del gasto obtenidos exitosamente"));
        }

        [HttpGet("student/{studentId}/pending")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StudentPaymentDto>>>> GetPendingPaymentsByStudentId(string studentId)
        {
            var payments = await _paymentService.GetPendingPaymentsByStudentIdAsync(studentId);
            return Ok(new ApiResponse<IEnumerable<StudentPaymentDto>>(payments, "Pagos pendientes del estudiante obtenidos exitosamente"));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<StudentPaymentDto>>> CreatePayment(CreateStudentPaymentDto dto)
        {
            try
            {
                var payment = await _paymentService.CreatePaymentAsync(dto);
                var response = new ApiResponse<StudentPaymentDto>(payment, "Pago creado exitosamente");
                return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<StudentPaymentDto>(null, ex.Message, false));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<StudentPaymentDto>>> UpdatePayment(string id, UpdateStudentPaymentDto dto)
        {
            try
            {
                var payment = await _paymentService.UpdatePaymentAsync(id, dto);
                return Ok(new ApiResponse<StudentPaymentDto>(payment, "Pago actualizado exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<StudentPaymentDto>(null, ex.Message, false));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeletePayment(string id)
        {
            try
            {
                await _paymentService.DeletePaymentAsync(id);
                return Ok(new ApiResponse<object>(null, "Pago eliminado exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(null, ex.Message, false));
            }
        }

        [HttpPut("register-payment-with-images")]
        public async Task<ActionResult<ApiResponse<StudentPaymentDto>>> RegisterPaymentWithImages([FromForm] RegisterPaymentWithImagesDto dto)
        {
            try
            {
                var result = await _paymentService.RegisterPaymentWithImagesAsync(dto);
                return Ok(new ApiResponse<StudentPaymentDto>(result, "Pago registrado exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<StudentPaymentDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentPaymentDto>(null, $"Error al registrar el pago: {ex.Message}", false));
            }
        }
    }
}
