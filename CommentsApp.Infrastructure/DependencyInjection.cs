using CommentsApp.Application.Common.Interfaces;
using CommentsApp.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommentsApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string uploadsPath)
        {
            services.AddMemoryCache();

            services.AddSingleton<ICaptchaService, CaptchaService>();
            services.AddSingleton<IFileService>(
                new FileService(uploadsPath));

            return services;
        }
    }
}
