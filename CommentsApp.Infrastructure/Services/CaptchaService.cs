using CommentsApp.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Infrastructure.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly IMemoryCache _cache;
        
        private const string CaptchaChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        
        public CaptchaService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public (string CaptchaId, byte[] ImageBytes) GenerateCaptcha()
        {
            var random = new Random();
            var captchaText = new string(
                Enumerable.Range(0, 6)
                    .Select(_ => CaptchaChars[random.Next(CaptchaChars.Length)])
                    .ToArray());

            var captchaId = Guid.NewGuid().ToString();

            _cache.Set($"captcha:{captchaId}", captchaText, TimeSpan.FromMinutes(5));

            var imageBytes = GenerateCaptchaImage(captchaText, random);

            return (captchaId, imageBytes);
        }

        public bool ValidateCaptcha(string captchaId, string userInput)
        {
            var cacheKey = $"captcha:{captchaId}";

            if (!_cache.TryGetValue(cacheKey, out string? correctText))
            {
                return false;
            }

            _cache.Remove(cacheKey);

            return string.Equals(correctText, userInput?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private byte[] GenerateCaptchaImage(string text, Random random)
        {
            int width = 200;
            int height = 70;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            canvas.Clear(new SKColor(
                (byte)random.Next(230, 255),
                (byte)random.Next(230, 255),
                (byte)random.Next(230, 255)));

            for (int i = 0; i < 8; i++)
            {
                using var linePaint = new SKPaint
                {
                    Color = new SKColor(
                        (byte)random.Next(100, 200),
                        (byte)random.Next(100, 200),
                        (byte)random.Next(100, 200)),
                    StrokeWidth = random.Next(1, 3),
                    IsAntialias = true
                };
                canvas.DrawLine(
                    random.Next(width),
                    random.Next(height),
                    random.Next(width),
                    random.Next(height),
                    linePaint);
            }

            for (int i = 0; i < 100; i++)
            {
                using var dotPaint = new SKPaint
                {
                    Color = new SKColor(
                        (byte)random.Next(80, 200),
                        (byte)random.Next(80, 200),
                        (byte)random.Next(80, 200))
                };
                canvas.DrawPoint(random.Next(width), random.Next(height), dotPaint);
            }

            float x = 15;
            foreach (var ch in text)
            {
                var typeface = SKTypeface.FromFamilyName(
                    "Arial",
                    random.Next(2) == 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                    SKFontStyleWidth.Normal,
                    SKFontStyleSlant.Upright);

                using var font = new SKFont(typeface, random.Next(28, 38));

                using var textPaint = new SKPaint
                {
                    IsAntialias = true,
                    Color = new SKColor(
                        (byte)random.Next(0, 100),
                        (byte)random.Next(0, 100),
                        (byte)random.Next(0, 100))
                };

                canvas.Save();
                canvas.RotateDegrees(random.Next(-15, 16), x + 12, height / 2f);

                canvas.DrawText(
                    ch.ToString(),
                    x,
                    height / 2f + font.Size / 3f,
                    SKTextAlign.Left,
                    font,
                    textPaint);
                canvas.Restore();

                x += font.MeasureText(ch.ToString()) + random.Next(2, 8);
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);

            return data.ToArray();
        }
    }
}
