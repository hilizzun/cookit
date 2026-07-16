using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reactive.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CookIt.Infrastructure.Services
{
    public class MinioImageStorage : IMinioImageStorage
    {
        private readonly IMinioClient _minioClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _bucketName = "images";

        public MinioImageStorage(
            IMinioClient minioClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _minioClient = minioClient;
            _httpContextAccessor = httpContextAccessor;

            InitializeBucketAsync().Wait();
        }

        private async Task InitializeBucketAsync()
        {
            try
            {
                var beArgs = new BucketExistsArgs()
                    .WithBucket(_bucketName);
                bool found = await _minioClient.BucketExistsAsync(beArgs);

                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(_bucketName);
                    await _minioClient.MakeBucketAsync(mbArgs);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize MinIO bucket: {ex.Message}");
            }
        }

        public async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var userId = GetCurrentUserId();
            var fileId = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var baseKey = $"users/{userId}/{fileId}";

            var originalKey = $"{baseKey}_original{extension}";
            var previewKey = $"{baseKey}_preview.jpg";

            using var stream = file.OpenReadStream();
            var putOriginalArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithStreamData(stream)
                .WithObject(originalKey)
                .WithObjectSize(stream.Length)
                .WithContentType(file.ContentType);
            await _minioClient.PutObjectAsync(putOriginalArgs);

            await CreateAndUploadPreviewAsync(file, previewKey);

            return baseKey;
        }

        private async Task CreateAndUploadPreviewAsync(IFormFile file, string previewKey)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var image = await Image.LoadAsync(stream);

                var resizeOptions = new ResizeOptions
                {
                    Size = new Size(700, 0),
                    Mode = ResizeMode.Max
                };

                image.Mutate(x => x.Resize(resizeOptions));

                using var outputStream = new MemoryStream();
                await image.SaveAsJpegAsync(outputStream);
                outputStream.Position = 0;

                var putPreviewArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithStreamData(outputStream)
                    .WithObject(previewKey)
                    .WithObjectSize(outputStream.Length)
                    .WithContentType("image/jpeg");
                await _minioClient.PutObjectAsync(putPreviewArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create preview: {ex.Message}");
            }
        }

        public async Task DeleteImageAsync(string imageKey)
        {
            if (string.IsNullOrEmpty(imageKey))
                return;

            try
            {
                await RemoveObjectsByPrefixAsync(imageKey);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete image: {ex.Message}");
            }
        }

        private async Task RemoveObjectsByPrefixAsync(string prefix)
        {
            var listArgs = new ListObjectsArgs()
                .WithBucket(_bucketName)
                .WithPrefix(prefix);

            var observable = _minioClient.ListObjectsAsync(listArgs);

            var items = await observable.ToList();

            var removeTasks = new List<Task>();
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    var removeArgs = new RemoveObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(item.Key);
                    removeTasks.Add(_minioClient.RemoveObjectAsync(removeArgs));
                }
            }

            await Task.WhenAll(removeTasks);
        }

        public async Task<string> GetPreviewUrlAsync(string imageKey)
        {
            if (string.IsNullOrEmpty(imageKey))
                return null;

            try
            {
                var previewKey = $"{imageKey}_preview.jpg";

                try
                {
                    var statArgs = new StatObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(previewKey);
                    await _minioClient.StatObjectAsync(statArgs);
                }
                catch
                {
                    previewKey = await FindOriginalObjectKeyAsync(imageKey);
                    if (previewKey == null)
                        return null;
                }

                var args = new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(previewKey)
                    .WithExpiry(3600);

                return await _minioClient.PresignedGetObjectAsync(args);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get preview URL: {ex.Message}");
            }
        }

        public async Task<string> GetOriginalUrlAsync(string imageKey)
        {
            if (string.IsNullOrEmpty(imageKey))
                return null;

            try
            {
                var originalKey = await FindOriginalObjectKeyAsync(imageKey);
                if (originalKey == null)
                    return null;

                var args = new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(originalKey)
                    .WithExpiry(3600);

                return await _minioClient.PresignedGetObjectAsync(args);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get original URL: {ex.Message}");
            }
        }

        private async Task<string> FindOriginalObjectKeyAsync(string baseKey)
        {
            var listArgs = new ListObjectsArgs()
                .WithBucket(_bucketName)
                .WithPrefix(baseKey);

            var observable = _minioClient.ListObjectsAsync(listArgs);
            var items = await observable.ToList();

            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Key.Contains("_original"))
                {
                    return item.Key;
                }
            }

            return null;
        }

        [Authorize]
        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "anonymous";
        }
    }
}