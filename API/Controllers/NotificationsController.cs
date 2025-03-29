using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Attributes;
using Application.DTOs.Notifications;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ICustomNotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            ICustomNotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las notificaciones
        /// </summary>
        [HttpGet]
        [AuthorizeRoles(UserRole.Administrator)]
        public async Task<ActionResult<List<NotificationDto>>> GetAll()
        {
            try
            {
                var notifications = await _notificationService.GetAllAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las notificaciones");
                return StatusCode(500, "Error interno del servidor al obtener las notificaciones");
            }
        }

        /// <summary>
        /// Obtiene una notificación por su ID
        /// </summary>
        [HttpGet("{id}")]
        [AuthorizeRoles(UserRole.Administrator)]
        public async Task<ActionResult<NotificationDto>> GetById(string id)
        {
            try
            {
                var notification = await _notificationService.GetByIdAsync(id);
                if (notification == null)
                {
                    return NotFound($"Notificación con ID {id} no encontrada");
                }
                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificación con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al obtener la notificación");
            }
        }

        /// <summary>
        /// Obtiene notificaciones por tópico
        /// </summary>
        [HttpGet("by-topic/{topic}")]
        [AuthorizeRoles(UserRole.Administrator)]
        public async Task<ActionResult<List<NotificationDto>>> GetByTopic(string topic)
        {
            try
            {
                var notifications = await _notificationService.GetByTopicAsync(topic);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones por tópico {Topic}", topic);
                return StatusCode(500, "Error interno del servidor al obtener las notificaciones por tópico");
            }
        }

        /// <summary>
        /// Obtiene notificaciones pendientes (no enviadas)
        /// </summary>
        [HttpGet("pending")]
        [AuthorizeRoles(UserRole.Administrator)]
        public async Task<ActionResult<List<NotificationDto>>> GetPending()
        {
            try
            {
                var notifications = await _notificationService.GetPendingNotificationsAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones pendientes");
                return StatusCode(500, "Error interno del servidor al obtener las notificaciones pendientes");
            }
        }

        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        [HttpPost]
        [AuthorizeRoles(UserRole.Administrator)]
        public async Task<ActionResult<string>> Create(CreateNotificationRequest request)
        {
            try
            {
                var id = await _notificationService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear notificación");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear notificación");
                return StatusCode(500, "Error interno del servidor al crear la notificación");
            }
        }

        /// <summary>
        /// Actualiza una notificación existente
        /// </summary>
        [HttpPut("{id}")]
        [AuthorizeRoles(UserRole.Administrator)]
        public async Task<ActionResult<NotificationDto>> Update(string id, UpdateNotificationRequest request)
        {
            try
            {
                var notification = await _notificationService.UpdateAsync(id, request);
                if (notification == null)
                {
                    return NotFound($"Notificación con ID {id} no encontrada");
                }
                return Ok(notification);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al actualizar notificación con ID {Id}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar notificación con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al actualizar la notificación");
            }
        }

        /// <summary>
        /// Elimina una notificación
        /// </summary>
        [HttpDelete("{id}")]
        [AuthorizeRoles(UserRole.Administrator)]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var result = await _notificationService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Notificación con ID {id} no encontrada");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al eliminar notificación con ID {Id}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar notificación con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al eliminar la notificación");
            }
        }

        /// <summary>
        /// Envía una notificación manualmente
        /// </summary>
        [HttpPost("{id}/send")]
        [AuthorizeRoles(UserRole.Administrator)]
        public async Task<ActionResult> SendNotification(string id)
        {
            try
            {
                var result = await _notificationService.SendNotificationAsync(id);
                if (!result)
                {
                    return BadRequest("No se pudo enviar la notificación. Verifique el ID y el estado.");
                }
                return Ok("Notificación enviada correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al enviar la notificación");
            }
        }

        /// <summary>
        /// Procesa todas las notificaciones programadas que estén pendientes
        /// </summary>
        [HttpPost("process-scheduled")]
        [AuthorizeRoles(UserRole.Administrator)]
        public async Task<ActionResult> ProcessScheduledNotifications()
        {
            try
            {
                var result = await _notificationService.ProcessScheduledNotificationsAsync();
                if (result)
                {
                    return Ok("Notificaciones programadas procesadas correctamente");
                }
                return Ok("No hay notificaciones programadas para procesar o hubo errores al enviar algunas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar notificaciones programadas");
                return StatusCode(500, "Error interno del servidor al procesar las notificaciones programadas");
            }
        }

        /// <summary>
        /// Obtiene las notificaciones para el usuario autenticado
        /// </summary>
        [HttpGet("my-notifications")]
        public async Task<ActionResult<List<NotificationDto>>> GetMyNotifications()
        {
            try
            {
                var userId = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("ID de usuario no encontrado en el token");
                }

                var notifications = await _notificationService.GetByUserIdAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones del usuario autenticado");
                return StatusCode(500, "Error interno del servidor al obtener las notificaciones");
            }
        }
    }
}
