using Microsoft.AspNetCore.Http;

namespace CookIt.Core.Interfaces
{
    public interface IMinioImageStorage
    {
        Task<string> SaveImageAsync(IFormFile imageFile);
        Task DeleteImageAsync(string imagePath);
        Task<string> GetPreviewUrlAsync(string imageKey);
        Task<string> GetOriginalUrlAsync(string imageKey);
    }
}