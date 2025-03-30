using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
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

        [HttpGet("collection/{collectionId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StudentPaymentDto>>>> GetPaymentsByCollectionId(string collectionId)
        {
            var payments = await _paymentService.GetPaymentsByCollectionIdAsync(collectionId);
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

        [HttpPut("update-payment-details/{id}")]
        public async Task<ActionResult<ApiResponse<StudentPaymentDto>>> UpdatePaymentDetails(string id, [FromBody] UpdatePaymentDetailsDto dto)
        {
            try
            {
                // Obtener el pago existente
                var existingPayment = await _paymentService.GetPaymentByIdAsync(id);
                
                // Crear un dto de actualización manteniendo el monto pagado original
                var updateDto = new UpdateStudentPaymentDto
                {
                    AmountPaid = existingPayment.AmountPaid,
                    Comment = dto.Comment,
                    Images = dto.Images,
                    PaymentDate = dto.PaymentDate
                };
                
                var payment = await _paymentService.UpdatePaymentAsync(id, updateDto);
                return Ok(new ApiResponse<StudentPaymentDto>(payment, "Detalles del pago actualizados exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<StudentPaymentDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentPaymentDto>(null, $"Error al actualizar detalles del pago: {ex.Message}", false));
            }
        }

        [HttpPut("update-payment-with-images/{id}")]
        public async Task<ActionResult<ApiResponse<StudentPaymentDto>>> UpdatePaymentWithImages(string id, [FromForm] UpdatePaymentImagesDto dto)
        {
            try
            {
                // Obtener el pago existente para mantener su monto
                var existingPayment = await _paymentService.GetPaymentByIdAsync(id);
                
                // Crear dto de registro manteniendo el monto pagado
                var registerDto = new RegisterPaymentWithImagesDto
                {
                    Id = id,
                    AmountPaid = existingPayment.AmountPaid,
                    Comment = dto.Comment,
                    Images = dto.Images,
                    PaymentDate = dto.PaymentDate
                };
                
                var result = await _paymentService.RegisterPaymentWithImagesAsync(registerDto);
                return Ok(new ApiResponse<StudentPaymentDto>(result, "Pago actualizado exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<StudentPaymentDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentPaymentDto>(null, $"Error al actualizar el pago: {ex.Message}", false));
            }
        }

        [HttpPut("exonerate/{id}")]
        public async Task<ActionResult<ApiResponse<StudentPaymentDto>>> ExoneratePayment(string id, [FromForm] ExoneratePaymentDto dto)
        {
            try
            {
                var result = await _paymentService.ExoneratePaymentAsync(id, dto);
                return Ok(new ApiResponse<StudentPaymentDto>(result, "Pago exonerado exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<StudentPaymentDto>(null, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<StudentPaymentDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentPaymentDto>(null, $"Error al exonerar el pago: {ex.Message}", false));
            }
        }
    }
}
