using CommentsApp.Application.Common.Interfaces;
using CommentsApp.Application.Common.Models;
using SkiaSharp;

namespace CommentsApp.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadsPath;

        private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif"
        };

        private const long MaxTextFileSize = 100 * 1024; // 100 KB

        private const int MaxImageWidth = 320;
        private const int MaxImageHeight = 240;

        public FileService(string uploadsPath)
        {
            _uploadsPath = uploadsPath;

            if (!Directory.Exists(_uploadsPath))
            {
                Directory.CreateDirectory(_uploadsPath);
            }
        }

        public async Task<FileUploadResult> SaveFileAsync(byte[] fileData, string fileName, string contentType)
        {
            var isImage = AllowedImageTypes.Contains(contentType);
            var isTextFile = contentType.Equals("text/plain", StringComparison.OrdinalIgnoreCase);

            if (!isImage && !isTextFile)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Only JPG, PNG, GIF images and TXT files are allowed."
                };
            }

            if (isTextFile && fileData.Length > MaxTextFileSize)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Text files must be less than 100 KB."
                };
            }

            byte[] dataToSave;

            if (isImage)
            {
                dataToSave = ResizeImageIfNeeded(fileData, contentType);
            }
            else
            {
                dataToSave = fileData;
            }

            var extension = Path.GetExtension(fileName).ToLower();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadsPath, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, dataToSave);

            return new FileUploadResult
            {
                Success = true,
                StoredFilePath = uniqueFileName,
                FileSize = dataToSave.Length
            };
        }

        private byte[] ResizeImageIfNeeded(byte[] imageData, string contentType)
        {
            using var inputStream = new MemoryStream(imageData);
            using var original = SKBitmap.Decode(inputStream);

            if (original == null)
                throw new InvalidOperationException("Failed to decode image.");

            if (original.Width <= MaxImageWidth && original.Height <= MaxImageHeight)
                return imageData;

            var ratioX = (float)MaxImageWidth / original.Width;
            var ratioY = (float)MaxImageHeight / original.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(original.Width * ratio);
            var newHeight = (int)(original.Height * ratio);

            using var resized = original.Resize(
                new SKImageInfo(newWidth, newHeight),
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

            if (resized == null)
                throw new InvalidOperationException("Failed to resize image.");

            using var image = SKImage.FromBitmap(resized);
            var format = contentType.ToLower() switch
            {
                "image/png" => SKEncodedImageFormat.Png,
                "image/gif" => SKEncodedImageFormat.Gif,
                _ => SKEncodedImageFormat.Jpeg
            };

            using var data = image.Encode(format, 90);

            return data.ToArray();
        }
    }
}
