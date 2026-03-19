using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Morourak.API.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Morourak API",
                    Version = "v1",
                    Description = "API for Morourak traffic services (Driving and Vehicle Licenses)."
                });

                var apiXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
                options.IncludeXmlComments(apiXmlPath);

                var applicationXmlFile = "Morourak.Application.xml";
                var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
                options.IncludeXmlComments(applicationXmlPath);
            });
            return services;
        }
    }
}
