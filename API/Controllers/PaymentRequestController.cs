using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaymentRequestController : ControllerBase
    {
        private readonly IPaymentRequestService _paymentRequestService;

        public PaymentRequestController(IPaymentRequestService paymentRequestService)
        {
            _paymentRequestService = paymentRequestService;
        }

        // Endpoints para representantes/padres

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentRequestDto>>>> GetRequestsByStudentId(string studentId)
        {
            var requests = await _paymentRequestService.GetPaymentRequestsByStudentIdAsync(studentId);
            return Ok(new ApiResponse<IEnumerable<PaymentRequestDto>>(requests, "Solicitudes de pago obtenidas exitosamente"));
        }

        [HttpGet("student/{studentId}/history")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentRequestDto>>>> GetRequestsHistoryByStudentId(string studentId)
        {
            var requests = await _paymentRequestService.GetRequestHistoryByStudentIdAsync(studentId);
            return Ok(new ApiResponse<IEnumerable<PaymentRequestDto>>(requests, "Historial de solicitudes obtenido exitosamente"));
        }

        [HttpPost("with-images")]
        public async Task<ActionResult<ApiResponse<PaymentRequestDto>>> CreatePaymentRequestWithImages([FromForm] CreatePaymentRequestWithImagesDto dto)
        {
            try
            {
                // Obtener el ID del usuario del token de autenticación (simulado aquí)
                string userId = User.Identity.Name ?? "user-id";
                
                var request = await _paymentRequestService.CreatePaymentRequestWithImagesAsync(dto, userId);
                var response = new ApiResponse<PaymentRequestDto>(request, "Solicitud de pago con imágenes creada exitosamente");
                return CreatedAtAction(nameof(GetRequestById), new { id = request.Id }, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PaymentRequestDto>(null, $"Error al crear la solicitud con imágenes: {ex.Message}", false));
            }
        }

        [HttpPut("with-images/{id}")]
        public async Task<ActionResult<ApiResponse<PaymentRequestDto>>> UpdatePaymentRequestWithImages(string id, [FromForm] UpdatePaymentRequestWithImagesDto dto)
        {
            try
            {
                // Obtener el ID del usuario del token de autenticación (simulado aquí)
                string userId = User.Identity.Name ?? "user-id";
                
                var request = await _paymentRequestService.UpdatePaymentRequestWithImagesAsync(id, dto, userId);
                return Ok(new ApiResponse<PaymentRequestDto>(request, "Solicitud de pago con imágenes actualizada exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PaymentRequestDto>(null, $"Error al actualizar la solicitud con imágenes: {ex.Message}", false));
            }
        }

        // Endpoints para administradores

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentRequestDto>>>> GetAllRequests()
        {
            var requests = await _paymentRequestService.GetAllPaymentRequestsAsync();
            return Ok(new ApiResponse<IEnumerable<PaymentRequestDto>>(requests, "Solicitudes de pago obtenidas exitosamente"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PaymentRequestDto>>> GetRequestById(string id)
        {
            try
            {
                var request = await _paymentRequestService.GetPaymentRequestByIdAsync(id);
                return Ok(new ApiResponse<PaymentRequestDto>(request, "Solicitud de pago obtenida exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentRequestDto>>>> GetRequestsByStatus(Domain.Entities.PaymentRequestStatus status)
        {
            var requests = await _paymentRequestService.GetPaymentRequestsByStatusAsync(status);
            return Ok(new ApiResponse<IEnumerable<PaymentRequestDto>>(requests, $"Solicitudes con estado {status} obtenidas exitosamente"));
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentRequestDto>>>> GetPendingRequests()
        {
            var requests = await _paymentRequestService.GetPendingRequestsAsync();
            return Ok(new ApiResponse<IEnumerable<PaymentRequestDto>>(requests, "Solicitudes pendientes obtenidas exitosamente"));
        }

        [HttpGet("under-review")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentRequestDto>>>> GetUnderReviewRequests()
        {
            var requests = await _paymentRequestService.GetUnderReviewRequestsAsync();
            return Ok(new ApiResponse<IEnumerable<PaymentRequestDto>>(requests, "Solicitudes en revisión obtenidas exitosamente"));
        }

        [HttpGet("needs-changes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentRequestDto>>>> GetNeedsChangesRequests()
        {
            var requests = await _paymentRequestService.GetNeedsChangesRequestsAsync();
            return Ok(new ApiResponse<IEnumerable<PaymentRequestDto>>(requests, "Solicitudes que requieren cambios obtenidas exitosamente"));
        }

        [HttpPut("{id}/approve")]
        public async Task<ActionResult<ApiResponse<PaymentRequestDto>>> ApproveRequest(string id, ApprovePaymentRequestDto dto)
        {
            try
            {
                var request = await _paymentRequestService.ApprovePaymentRequestAsync(id, dto);
                return Ok(new ApiResponse<PaymentRequestDto>(request, "Solicitud de pago aprobada exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PaymentRequestDto>(null, $"Error al aprobar la solicitud: {ex.Message}", false));
            }
        }

        [HttpPut("{id}/reject")]
        public async Task<ActionResult<ApiResponse<PaymentRequestDto>>> RejectRequest(string id, RejectPaymentRequestDto dto)
        {
            try
            {
                var request = await _paymentRequestService.RejectPaymentRequestAsync(id, dto);
                return Ok(new ApiResponse<PaymentRequestDto>(request, "Solicitud de pago rechazada"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PaymentRequestDto>(null, $"Error al rechazar la solicitud: {ex.Message}", false));
            }
        }

        [HttpPut("{id}/request-changes")]
        public async Task<ActionResult<ApiResponse<PaymentRequestDto>>> RequestChanges(string id, RequestChangesDto dto)
        {
            try
            {
                var request = await _paymentRequestService.RequestChangesAsync(id, dto);
                return Ok(new ApiResponse<PaymentRequestDto>(request, "Se han solicitado cambios exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PaymentRequestDto>(null, $"Error al solicitar cambios: {ex.Message}", false));
            }
        }

        [HttpPost("{id}/comment")]
        public async Task<ActionResult<ApiResponse<PaymentRequestDto>>> AddAdminComment(string id, AddAdminCommentDto dto)
        {
            try
            {
                var request = await _paymentRequestService.AddAdminCommentAsync(id, dto);
                return Ok(new ApiResponse<PaymentRequestDto>(request, "Comentario agregado exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<PaymentRequestDto>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PaymentRequestDto>(null, $"Error al agregar el comentario: {ex.Message}", false));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteRequest(string id)
        {
            try
            {
                await _paymentRequestService.DeletePaymentRequestAsync(id);
                return Ok(new ApiResponse<object>(null, "Solicitud de pago eliminada exitosamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(null, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>(null, ex.Message, false));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(null, $"Error al eliminar la solicitud: {ex.Message}", false));
            }
        }
    }
} 