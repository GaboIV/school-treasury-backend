using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _baseUrl;
        private readonly ILogger<FileService> _logger;
        private const int ThumbnailWidth = 150;
        private const int ImageMaxWidth = 1200;
        private const int CompressionQuality = 80;

        public FileService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, ILogger<FileService> logger = null)
        {
            _environment = environment;
            _logger = logger;
            
            // Construir la URL base para las imágenes
            var request = httpContextAccessor.HttpContext?.Request;
            _baseUrl = $"{request?.Scheme}://{request?.Host}";
        }

        public async Task<List<string>> SaveImagesAsync(List<IFormFile> images, string folder, string itemId = null)
        {
            var savedPaths = new List<string>();
            
            // Crear directorio base
            var basePath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", folder);
            
            // Si hay un ID de elemento, crear una subcarpeta específica
            string itemFolder = string.Empty;
            if (!string.IsNullOrEmpty(itemId))
            {
                itemFolder = itemId;
                basePath = Path.Combine(basePath, itemFolder);
            }
            
            // Crear directorio para imágenes originales
            var uploadPath = basePath;
            var thumbnailPath = Path.Combine(basePath, "thumbnails");
            
            LogInfo($"Intentando crear directorio en {uploadPath}");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
                LogInfo($"Directorio {uploadPath} creado con éxito.");
            }
            
            if (!Directory.Exists(thumbnailPath))
            {
                Directory.CreateDirectory(thumbnailPath);
                LogInfo($"Directorio de miniaturas {thumbnailPath} creado con éxito.");
            }

            LogInfo("Cantidad de imágenes: " + images.Count);

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    try
                    {
                        // Generar nombre único para la imagen
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                        var originalFilePath = Path.Combine(uploadPath, fileName);
                        var thumbnailFilePath = Path.Combine(thumbnailPath, fileName);
                        
                        LogInfo($"Procesando imagen: {fileName}");
                        
                        // Leer la imagen completamente a memoria antes de procesarla
                        byte[] imageData;
                        using (var memStream = new MemoryStream())
                        {
                            await image.CopyToAsync(memStream);
                            imageData = memStream.ToArray();
                        }
                        
                        try
                        {
                            // Procesar la imagen para comprimir y redimensionar
                            using (var imageStream = new MemoryStream(imageData))
                            {
                                await CompressAndResizeImage(imageStream, originalFilePath, ImageMaxWidth, CompressionQuality);
                                LogInfo($"Imagen comprimida guardada en {originalFilePath}");
                            }
                            
                            // Procesar la imagen para la miniatura usando una nueva copia
                            using (var thumbnailStream = new MemoryStream(imageData))
                            {
                                await CreateThumbnail(thumbnailStream, thumbnailFilePath, ThumbnailWidth, CompressionQuality);
                                LogInfo($"Miniatura creada en {thumbnailFilePath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError($"Error al procesar la imagen: {ex.Message}");
                            LogError($"StackTrace: {ex.StackTrace}");
                            continue;
                        }
                        
                        // Guardar ruta relativa según la estructura de carpetas
                        string relativePath;
                        
                        if (!string.IsNullOrEmpty(itemFolder))
                        {
                            relativePath = $"/uploads/{folder}/{itemFolder}/{fileName}";
                        }
                        else
                        {
                            relativePath = $"/uploads/{folder}/{fileName}";
                        }
                        
                        savedPaths.Add(relativePath);
                        LogInfo($"Ruta relativa de la imagen: {relativePath}");
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error general al procesar imagen: {ex.Message}");
                        LogError($"StackTrace: {ex.StackTrace}");
                    }
                }
                else
                {
                    LogInfo($"Imagen vacía, no se guardará.");
                }
            }
            
            return savedPaths;
        }

        private async Task CompressAndResizeImage(Stream imageStream, string outputPath, int maxWidth, int quality)
        {
            // Asegurarse de que el stream está en la posición correcta
            imageStream.Position = 0;
            
            using (var original = SKBitmap.Decode(imageStream))
            {
                if (original == null)
                {
                    throw new Exception("No se pudo decodificar la imagen");
                }

                // Calcular nuevo tamaño
                var width = original.Width;
                var height = original.Height;
                
                if (width > maxWidth)
                {
                    var ratio = (float)maxWidth / width;
                    width = maxWidth;
                    height = (int)(height * ratio);
                }
                
                try
                {
                    using (var resized = original.Width == width && original.Height == height 
                        ? original 
                        : original.Resize(new SKImageInfo(width, height), SKFilterQuality.High))
                    {
                        using (var image = SKImage.FromBitmap(resized))
                        using (var encodedData = image.Encode(SKEncodedImageFormat.Jpeg, quality))
                        {
                            using (var fileStream = File.Create(outputPath))
                            {
                                encodedData.SaveTo(fileStream);
                                await fileStream.FlushAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al redimensionar o guardar la imagen: {ex.Message}", ex);
                }
            }
        }

        private async Task CreateThumbnail(Stream imageStream, string outputPath, int width, int quality)
        {
            // Asegurarse de que el stream está en la posición correcta
            imageStream.Position = 0;
            
            using (var original = SKBitmap.Decode(imageStream))
            {
                if (original == null)
                {
                    throw new Exception("No se pudo decodificar la imagen para la miniatura");
                }
                
                try
                {
                    // Calcular la altura proporcional
                    var ratio = (float)width / original.Width;
                    var height = (int)(original.Height * ratio);
                    
                    using (var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High))
                    {
                        using (var image = SKImage.FromBitmap(resized))
                        using (var encodedData = image.Encode(SKEncodedImageFormat.Jpeg, quality))
                        {
                            using (var fileStream = File.Create(outputPath))
                            {
                                encodedData.SaveTo(fileStream);
                                await fileStream.FlushAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al crear la miniatura: {ex.Message}", ex);
                }
            }
        }

        public void DeleteImages(List<string> imagePaths)
        {
            foreach (var path in imagePaths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                
                try
                {
                    // Convertir ruta relativa a absoluta
                    var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", path.TrimStart('/'));
                    
                    // Determinar la ruta de la miniatura basada en la estructura de carpetas
                    string thumbnailPath;
                    
                    // Extraer componentes de la ruta
                    var dirName = Path.GetDirectoryName(path.TrimStart('/'));
                    var fileName = Path.GetFileName(path);
                    
                    if (dirName.Contains('/'))
                    {
                        // Para rutas con estructura /uploads/folder/itemId/
                        thumbnailPath = Path.Combine(_environment.ContentRootPath, "wwwroot", dirName, "thumbnails", fileName);
                    }
                    else
                    {
                        // Para rutas con estructura /uploads/folder/
                        thumbnailPath = Path.Combine(_environment.ContentRootPath, "wwwroot", dirName, "thumbnails", fileName);
                    }
                    
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        LogInfo($"Imagen eliminada: {fullPath}");
                    }
                    
                    if (File.Exists(thumbnailPath))
                    {
                        File.Delete(thumbnailPath);
                        LogInfo($"Miniatura eliminada: {thumbnailPath}");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error al eliminar imagen: {ex.Message}");
                }
            }
        }

        public string GetImageUrl(string imagePath, bool thumbnail = false)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;

            // Si ya es una URL completa, devolverla tal cual
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;
            
            // Verificar si tenemos un nombre de archivo sin ruta
            // Si no contiene barras, podría ser solo un nombre de archivo
            if (!imagePath.Contains("/"))
            {
                // Asumir que es un nombre de archivo que debería estar en /uploads/expenses/
                imagePath = $"/uploads/expenses/{imagePath}";
            }
            
            // Verificar si tenemos una ruta relativa con formato correcto
            if (!imagePath.StartsWith("/"))
            {
                imagePath = $"/{imagePath}";
            }
            
            // Si se solicita miniatura, modificar la ruta
            if (thumbnail && !imagePath.Contains("/thumbnails/"))
            {
                var lastSlashPos = imagePath.LastIndexOf('/');
                if (lastSlashPos > 0)
                {
                    var fileName = imagePath.Substring(lastSlashPos + 1);
                    var basePath = imagePath.Substring(0, lastSlashPos);
                    
                    imagePath = $"{basePath}/thumbnails/{fileName}";
                }
            }

            LogInfo($"URL generada para {imagePath}: {_baseUrl}{imagePath}");
            return $"{_baseUrl}{imagePath}";
        }

        public string GetThumbnailUrl(string imagePath)
        {
            return GetImageUrl(imagePath, true);
        }

        public bool ImageExists(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            // Si solo es un nombre de archivo sin ruta
            if (!imagePath.Contains("/"))
            {
                // Asumir que está en /uploads/expenses/
                imagePath = $"/uploads/expenses/{imagePath}";
            }

            // Convertir ruta relativa a absoluta
            var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", imagePath.TrimStart('/'));
            
            var exists = File.Exists(fullPath);
            if (!exists)
            {
                LogError($"Imagen no encontrada en la ruta: {fullPath}");
            }
            return exists;
        }
        
        private void LogInfo(string message)
        {
            _logger?.LogInformation(message);
            Console.WriteLine(message);
        }
        
        private void LogError(string message)
        {
            _logger?.LogError(message);
            Console.WriteLine($"ERROR: {message}");
        }
    }
} 