using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CookIt.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IMinioImageStorage _imageStorage;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(
            IMinioImageStorage imageStorage,
            ILogger<ImagesController> logger)
        {
            _imageStorage = imageStorage;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            _logger.LogInformation(
                "Загрузка изображения. Имя файла: {FileName}, Размер: {FileSize}, Тип: {ContentType}",
                file?.FileName, file?.Length, file?.ContentType);

            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Пустой файл при загрузке изображения");
                    return BadRequest(new { Error = "File is empty" });
                }

                var imageKey = await _imageStorage.SaveImageAsync(file);

                _logger.LogInformation(
                    "Изображение успешно загружено. Ключ: {ImageKey}, Размер: {FileSize}",
                    imageKey, file.Length);

                return Ok(new
                {
                    Key = imageKey,
                    Message = "Image uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке изображения");
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("delete/{imageKey}")]
        public async Task<IActionResult> Delete(string imageKey)
        {
            _logger.LogInformation("Удаление изображения. Ключ: {ImageKey}", imageKey);

            try
            {
                await _imageStorage.DeleteImageAsync(imageKey);

                _logger.LogInformation("Изображение успешно удалено. Ключ: {ImageKey}", imageKey);

                return Ok(new { Message = "Image deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении изображения. Ключ: {ImageKey}", imageKey);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("preview/{*imageKey}")]
        public async Task<IActionResult> GetPreviewUrl(string imageKey)
        {
            _logger.LogDebug("Запрос URL предпросмотра изображения. Ключ: {ImageKey}", imageKey);

            try
            {
                var url = await _imageStorage.GetPreviewUrlAsync(imageKey);
                if (string.IsNullOrEmpty(url))
                {
                    _logger.LogWarning("Изображение не найдено при запросе предпросмотра. Ключ: {ImageKey}", imageKey);
                    return NotFound(new { Error = "Image not found" });
                }

                _logger.LogDebug(
                    "URL предпросмотра сгенерирован. Ключ: {ImageKey}, URL: {Url}",
                    imageKey, url);

                return Ok(new { Url = url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении URL предпросмотра. Ключ: {ImageKey}", imageKey);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("original/{*imageKey}")]
        public async Task<IActionResult> GetOriginalUrl(string imageKey)
        {
            _logger.LogDebug("Запрос оригинального URL изображения. Ключ: {ImageKey}", imageKey);

            try
            {
                var url = await _imageStorage.GetOriginalUrlAsync(imageKey);
                if (string.IsNullOrEmpty(url))
                {
                    _logger.LogWarning("Изображение не найдено при запросе оригинального URL. Ключ: {ImageKey}", imageKey);
                    return NotFound(new { Error = "Image not found" });
                }

                _logger.LogDebug(
                    "Оригинальный URL сгенерирован. Ключ: {ImageKey}, URL: {Url}",
                    imageKey, url);

                return Ok(new { Url = url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении оригинального URL. Ключ: {ImageKey}", imageKey);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("preview-proxy/{*imageKey}")]
        public async Task<IActionResult> PreviewProxy(string imageKey)
        {
            _logger.LogDebug("Проксирование предпросмотра изображения. Ключ: {ImageKey}", imageKey);

            try
            {
                var url = await _imageStorage.GetPreviewUrlAsync(imageKey);
                if (string.IsNullOrEmpty(url))
                {
                    _logger.LogWarning("Изображение не найдено для проксирования. Ключ: {ImageKey}", imageKey);
                    return NotFound();
                }

                _logger.LogDebug(
                    "Проксирование на URL: {Url} для изображения {ImageKey}",
                    url, imageKey);

                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проксировании изображения. Ключ: {ImageKey}", imageKey);
                return NotFound();
            }
        }
    }
}