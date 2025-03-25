using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFileService
    {
        Task<List<string>> SaveImagesAsync(List<IFormFile> images, string folder, string itemId = null);
        void DeleteImages(List<string> imagePaths);
        string GetImageUrl(string imagePath, bool thumbnail = false);
        string GetThumbnailUrl(string imagePath);
        bool ImageExists(string imagePath);
    }
} 