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
        private const int ImageMaxWidth = 1024;
        private const int ImageMaxHeight = 1024;
        private const int CompressionQuality = 70;

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
            try
            {
                // Copiar todo el stream a memoria para evitar problemas con streams cerrados
                byte[] imageData;
                using (var memStream = new MemoryStream())
                {
                    imageStream.Position = 0;
                    await imageStream.CopyToAsync(memStream);
                    imageData = memStream.ToArray();
                }

                // Usar una nueva copia del stream para decodificar la imagen
                using (var imageDataStream = new MemoryStream(imageData))
                using (var original = SKBitmap.Decode(imageDataStream))
                {
                    if (original == null)
                    {
                        throw new Exception("No se pudo decodificar la imagen");
                    }

                    try
                    {
                        // Extraer la orientación EXIF para corregir la rotación
                        using (var exifStream = new MemoryStream(imageData))
                        using (var codec = SKCodec.Create(exifStream))
                        {
                            var orientation = codec?.EncodedOrigin ?? SKEncodedOrigin.Default;
                            LogInfo($"Orientación EXIF detectada: {orientation}. Tamaño original: {original.Width}x{original.Height}");

                            // Calcular nuevo tamaño respetando la relación de aspecto
                            int width = original.Width;
                            int height = original.Height;

                            // Aplicar restricciones de tamaño máximo, manteniendo proporción
                            float scaleRatio = 1.0f;

                            // Si el ancho excede el máximo
                            if (width > ImageMaxWidth)
                            {
                                float widthRatio = (float)ImageMaxWidth / width;
                                scaleRatio = Math.Min(scaleRatio, widthRatio);
                            }

                            // Si el alto excede el máximo
                            if (height > ImageMaxHeight)
                            {
                                float heightRatio = (float)ImageMaxHeight / height;
                                scaleRatio = Math.Min(scaleRatio, heightRatio);
                            }

                            // Si necesitamos escalar
                            if (scaleRatio < 1.0f)
                            {
                                width = (int)(width * scaleRatio);
                                height = (int)(height * scaleRatio);
                                LogInfo($"Imagen redimensionada a: {width}x{height} (ratio: {scaleRatio})");
                            }
                            else
                            {
                                LogInfo("No se necesita redimensionar la imagen");
                            }

                            // Verificar si necesitamos rotar basado en la orientación EXIF
                            bool needsTransform = orientation != SKEncodedOrigin.Default && orientation != SKEncodedOrigin.TopLeft;

                            // Redimensionar la imagen manteniendo la calidad
                            using (var resized = original.Width == width && original.Height == height && !needsTransform
                                ? original
                                : original.Resize(new SKImageInfo(width, height), SKFilterQuality.High))
                            {
                                // Aplicar transformación basada en la orientación EXIF
                                SKBitmap transformedBitmap = null;

                                try
                                {
                                    if (needsTransform)
                                    {
                                        LogInfo($"Aplicando transformación para orientación: {orientation}");
                                        transformedBitmap = ApplyOrientation(resized, orientation);
                                    }
                                    else
                                    {
                                        LogInfo("No se requiere transformación de orientación");
                                        transformedBitmap = resized;
                                    }

                                    LogInfo($"Codificando imagen con calidad: {quality}%. Tamaño: {transformedBitmap.Width}x{transformedBitmap.Height}");
                                    using (var image = SKImage.FromBitmap(transformedBitmap))
                                    using (var encodedData = image.Encode(SKEncodedImageFormat.Jpeg, quality))
                                    {
                                        LogInfo($"Guardando imagen en: {outputPath}. Tamaño: {encodedData.Size} bytes");
                                        using (var fileStream = File.Create(outputPath))
                                        {
                                            encodedData.SaveTo(fileStream);
                                            await fileStream.FlushAsync();
                                        }
                                    }
                                }
                                finally
                                {
                                    // Liberar recursos si creamos un nuevo bitmap
                                    if (transformedBitmap != null && transformedBitmap != resized && transformedBitmap != original)
                                    {
                                        transformedBitmap.Dispose();
                                    }
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
            catch (Exception ex)
            {
                throw new Exception($"Error al procesar la imagen: {ex.Message}", ex);
            }
        }

        private async Task CreateThumbnail(Stream imageStream, string outputPath, int width, int quality)
        {
            try
            {
                // Copiar todo el stream a memoria para evitar problemas con streams cerrados
                byte[] imageData;
                using (var memStream = new MemoryStream())
                {
                    imageStream.Position = 0;
                    await imageStream.CopyToAsync(memStream);
                    imageData = memStream.ToArray();
                }

                // Usar una nueva copia del stream para decodificar la imagen
                using (var imageDataStream = new MemoryStream(imageData))
                using (var original = SKBitmap.Decode(imageDataStream))
                {
                    if (original == null)
                    {
                        throw new Exception("No se pudo decodificar la imagen para la miniatura");
                    }

                    try
                    {
                        // Extraer la orientación EXIF para corregir la rotación
                        using (var exifStream = new MemoryStream(imageData))
                        using (var codec = SKCodec.Create(exifStream))
                        {
                            var orientation = codec?.EncodedOrigin ?? SKEncodedOrigin.Default;
                            LogInfo($"Miniatura - Orientación EXIF detectada: {orientation}. Tamaño original: {original.Width}x{original.Height}");

                            // Calcular nueva dimensión conservando proporción
                            int thumbWidth, thumbHeight;
                            float aspectRatio = (float)original.Width / original.Height;
                            LogInfo($"Miniatura - Relación de aspecto: {aspectRatio}");

                            // Si es una imagen horizontal
                            if (original.Width >= original.Height)
                            {
                                thumbWidth = width;
                                thumbHeight = (int)(width / aspectRatio);
                                LogInfo($"Miniatura - Imagen horizontal redimensionada a: {thumbWidth}x{thumbHeight}");
                            }
                            // Si es una imagen vertical
                            else
                            {
                                thumbHeight = width;
                                thumbWidth = (int)(width * aspectRatio);
                                LogInfo($"Miniatura - Imagen vertical redimensionada a: {thumbWidth}x{thumbHeight}");
                            }

                            // Verificar si necesitamos rotar basado en la orientación EXIF
                            bool needsTransform = orientation != SKEncodedOrigin.Default && orientation != SKEncodedOrigin.TopLeft;

                            using (var resized = original.Resize(new SKImageInfo(thumbWidth, thumbHeight), SKFilterQuality.High))
                            {
                                // Aplicar transformación basada en la orientación EXIF
                                SKBitmap transformedBitmap = null;

                                try
                                {
                                    if (needsTransform)
                                    {
                                        LogInfo($"Miniatura - Aplicando transformación para orientación: {orientation}");
                                        transformedBitmap = ApplyOrientation(resized, orientation);
                                    }
                                    else
                                    {
                                        LogInfo("Miniatura - No se requiere transformación de orientación");
                                        transformedBitmap = resized;
                                    }

                                    LogInfo($"Miniatura - Codificando imagen con calidad: {quality}%. Tamaño: {transformedBitmap.Width}x{transformedBitmap.Height}");
                                    using (var image = SKImage.FromBitmap(transformedBitmap))
                                    using (var encodedData = image.Encode(SKEncodedImageFormat.Jpeg, quality))
                                    {
                                        LogInfo($"Miniatura - Guardando en: {outputPath}. Tamaño: {encodedData.Size} bytes");
                                        using (var fileStream = File.Create(outputPath))
                                        {
                                            encodedData.SaveTo(fileStream);
                                            await fileStream.FlushAsync();
                                        }
                                    }
                                }
                                finally
                                {
                                    // Liberar recursos si creamos un nuevo bitmap
                                    if (transformedBitmap != null && transformedBitmap != resized && transformedBitmap != original)
                                    {
                                        transformedBitmap.Dispose();
                                    }
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
            catch (Exception ex)
            {
                throw new Exception($"Error al procesar la miniatura: {ex.Message}", ex);
            }
        }

        // Método auxiliar para aplicar la orientación EXIF
        private SKBitmap ApplyOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
        {
            LogInfo($"Aplicando orientación EXIF: {origin}");

            if (origin == SKEncodedOrigin.Default || origin == SKEncodedOrigin.TopLeft)
            {
                // No rotation needed for Default/TopLeft
                return bitmap;
            }

            SKBitmap? rotated = null;

            try
            {
                switch (origin)
                {
                    case SKEncodedOrigin.TopRight: // Mirror horizontal
                        rotated = new SKBitmap(bitmap.Width, bitmap.Height);
                        using (var canvas = new SKCanvas(rotated))
                        {
                            canvas.Scale(-1, 1);
                            canvas.Translate(-bitmap.Width, 0);
                            canvas.DrawBitmap(bitmap, 0, 0);
                        }
                        break;

                    case SKEncodedOrigin.BottomRight: // Rotate 180
                        rotated = new SKBitmap(bitmap.Width, bitmap.Height);
                        using (var canvas = new SKCanvas(rotated))
                        {
                            canvas.RotateDegrees(180, bitmap.Width / 2, bitmap.Height / 2);
                            canvas.DrawBitmap(bitmap, 0, 0);
                        }
                        break;

                    case SKEncodedOrigin.BottomLeft: // Mirror vertical
                        rotated = new SKBitmap(bitmap.Width, bitmap.Height);
                        using (var canvas = new SKCanvas(rotated))
                        {
                            canvas.Scale(1, -1);
                            canvas.Translate(0, -bitmap.Height);
                            canvas.DrawBitmap(bitmap, 0, 0);
                        }
                        break;

                    case SKEncodedOrigin.LeftTop: // Rotate 90 + mirror horizontal
                        rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                        using (var canvas = new SKCanvas(rotated))
                        {
                            canvas.Translate(rotated.Width, 0);
                            canvas.Scale(-1, 1);
                            canvas.RotateDegrees(90);
                            canvas.DrawBitmap(bitmap, 0, 0);
                        }
                        break;

                    case SKEncodedOrigin.RightTop: // Rotate 90
                        rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                        using (var canvas = new SKCanvas(rotated))
                        {
                            canvas.Translate(rotated.Width, 0);
                            canvas.RotateDegrees(90);
                            canvas.DrawBitmap(bitmap, 0, 0);
                        }
                        break;

                    case SKEncodedOrigin.RightBottom: // Mirror horizontal + rotate 90
                        rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                        using (var canvas = new SKCanvas(rotated))
                        {
                            canvas.Scale(-1, 1);
                            canvas.Translate(-rotated.Width, 0);
                            canvas.Translate(0, rotated.Height);
                            canvas.RotateDegrees(-90);
                            canvas.DrawBitmap(bitmap, 0, 0);
                        }
                        break;

                    case SKEncodedOrigin.LeftBottom: // Rotate 270
                        rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                        using (var canvas = new SKCanvas(rotated))
                        {
                            canvas.Translate(0, rotated.Height);
                            canvas.RotateDegrees(-90);
                            canvas.DrawBitmap(bitmap, 0, 0);
                        }
                        break;

                    default:
                        return bitmap;
                }

                LogInfo($"Orientación aplicada correctamente: {origin}. Tamaño original: {bitmap.Width}x{bitmap.Height}, Nuevo: {rotated.Width}x{rotated.Height}");
                return rotated;
            }
            catch (Exception ex)
            {
                LogError($"Error al aplicar orientación {origin}: {ex.Message}");
                // Si falla, devolver el bitmap original sin rotar
                return bitmap;
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

            // Si ya es una URL completa, convertirla a https si es necesario
            if (imagePath.StartsWith("http://"))
                return imagePath.Replace("http://", "https://");
            if (imagePath.StartsWith("https://"))
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
            return $"{_baseUrl.Replace("http://", "https://")}{imagePath}";
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