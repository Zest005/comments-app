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

        private const long MaxTextFileSize = 100 * 1024;
        private const int MaxImageWidth = 320;
        private const int MaxImageHeight = 240;

        private static readonly byte[] JpegMagic = { 0xFF, 0xD8, 0xFF };
        private static readonly byte[] PngMagic = { 0x89, 0x50, 0x4E, 0x47 };
        private static readonly byte[] GifMagic87 = { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }; // GIF87a
        private static readonly byte[] GifMagic89 = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // GIF89a

        public FileService(string uploadsPath)
        {
            _uploadsPath = uploadsPath;
            if (!Directory.Exists(_uploadsPath))
                Directory.CreateDirectory(_uploadsPath);
        }

        public async Task<FileUploadResult> SaveFileAsync(
            byte[] fileData, string fileName, string contentType)
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

            if (isImage && !IsValidImage(fileData))
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "File content does not match a valid image format (JPG, PNG, GIF)."
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
                try
                {
                    dataToSave = ResizeImageIfNeeded(fileData, contentType);
                }
                catch (Exception)
                {
                    return new FileUploadResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to process image. The file may be corrupted."
                    };
                }
            }
            else
            {
                dataToSave = fileData;
            }

            var extension = Path.GetExtension(fileName).ToLower();
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".txt" };
            if (!allowedExtensions.Contains(extension))
                extension = isImage ? ".jpg" : ".txt";

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

        private static bool IsValidImage(byte[] data)
        {
            if (data.Length < 4) return false;

            return StartsWith(data, JpegMagic)
                || StartsWith(data, PngMagic)
                || StartsWith(data, GifMagic87)
                || StartsWith(data, GifMagic89);
        }

        private static bool StartsWith(byte[] data, byte[] magic)
        {
            if (data.Length < magic.Length) return false;
            for (int i = 0; i < magic.Length; i++)
            {
                if (data[i] != magic[i]) return false;
            }
            return true;
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