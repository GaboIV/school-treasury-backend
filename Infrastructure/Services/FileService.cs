using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _baseUrl;

        public FileService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            
            // Construir la URL base para las imágenes
            var request = httpContextAccessor.HttpContext?.Request;
            _baseUrl = $"{request?.Scheme}://{request?.Host}";
        }

        public async Task<List<string>> SaveImagesAsync(List<IFormFile> images, string folder)
        {
            var savedPaths = new List<string>();
            
            // Crear directorio si no existe
            var uploadPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", folder);
            Console.WriteLine($"Intentando crear directorio en {uploadPath}");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
                Console.WriteLine($"Directorio {uploadPath} creado con éxito.");
            }
            else
            {
                Console.WriteLine($"El directorio {uploadPath} ya existe.");
            }

            Console.WriteLine("Cantidad de imágenes: " + images.Count);

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    // Generar nombre único para la imagen
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);
                    Console.WriteLine($"Intentando guardar imagen en {filePath}");
                    
                    // Guardar la imagen
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                        Console.WriteLine($"Imagen guardada en {filePath} con éxito.");
                    }
                    
                    // Guardar la ruta relativa
                    var relativePath = $"/uploads/{folder}/{fileName}";
                    savedPaths.Add(relativePath);
                    Console.WriteLine($"Ruta relativa de la imagen: {relativePath}");
                }
                else
                {
                    Console.WriteLine($"Imagen vacía, no se guardará.");
                }
            }
            
            return savedPaths;
        }

        public void DeleteImages(List<string> imagePaths)
        {
            foreach (var path in imagePaths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                
                // Convertir ruta relativa a absoluta
                var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", path.TrimStart('/'));
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }

        public string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;

            // Si ya es una URL completa, devolverla tal cual
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;
                
            // Si es solo el nombre del archivo, añadir la ruta /uploads/expenses/
            if (!imagePath.Contains("/") && !imagePath.StartsWith("/uploads/"))
            {
                // Determinar la carpeta basada en alguna convención o patrón en el nombre
                string folder = "expenses"; // Por defecto
                
                // Asegurarse de que la ruta comienza con /
                imagePath = $"/uploads/{folder}/{imagePath}";
            }
            // Si ya tiene /uploads/ pero no comienza con /, añadir /
            else if (imagePath.Contains("/uploads/") && !imagePath.StartsWith("/"))
            {
                imagePath = $"/{imagePath}";
            }
            // Si no tiene /uploads/ pero comienza con /, podría ser solo el nombre del archivo
            else if (!imagePath.Contains("/uploads/") && imagePath.StartsWith("/"))
            {
                // Extraer solo el nombre del archivo si hay una ruta
                string fileName = Path.GetFileName(imagePath);
                imagePath = $"/uploads/expenses/{fileName}";
            }

            return $"{_baseUrl}{imagePath}";
        }

        public bool ImageExists(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            // Convertir ruta relativa a absoluta
            var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", imagePath.TrimStart('/'));
            
            return File.Exists(fullPath);
        }
    }
} 