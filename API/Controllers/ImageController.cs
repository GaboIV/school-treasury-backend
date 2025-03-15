using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _environment;

        public ImageController(IFileService fileService, IWebHostEnvironment environment)
        {
            _fileService = fileService;
            _environment = environment;
        }

        [HttpGet("{*imagePath}")]
        public IActionResult GetImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return BadRequest(new ApiResponse<object>(null, "Ruta de imagen no proporcionada", false));
                }

                // Normalizar la ruta
                imagePath = imagePath.TrimStart('/');
                
                // Verificar si la imagen existe
                if (!_fileService.ImageExists(imagePath))
                {
                    return NotFound(new ApiResponse<object>(null, "Imagen no encontrada", false));
                }

                // Obtener la ruta completa del archivo
                var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", imagePath);
                
                // Determinar el tipo de contenido basado en la extensión del archivo
                var contentType = GetContentType(Path.GetExtension(fullPath));
                
                // Leer y devolver el archivo
                var fileBytes = System.IO.File.ReadAllBytes(fullPath);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(null, $"Error al obtener la imagen: {ex.Message}", false));
            }
        }

        private string GetContentType(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                case ".pdf":
                    return "application/pdf";
                default:
                    return "application/octet-stream";
            }
        }
    }
} 