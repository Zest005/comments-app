using CommentsApp.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CommentsApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CommentService>();

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
