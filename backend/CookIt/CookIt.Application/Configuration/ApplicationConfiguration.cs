using CookIt.Application.Services;
using CookIt.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookIt.Application.Configuration
{
    public static class ApplicationConfiguration
    {
        public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRecipeService, RecipeService>(); 
        }
    }
}
