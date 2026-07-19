using AutoMapper;
using CookIt.Core.Interfaces;
using CookIt.Infrastructure.Configuration.Profiles;
using CookIt.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CookIt.Infrastructure.Configuration
{
    public static class InfrastructureConfiguration
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddDbContext<CookItContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            services.AddScoped<DbContext>(provider =>
                provider.GetRequiredService<CookItContext>());

            services.AddScoped<IRecipeRepository, RecipeRepository>();

            services.AddAutoMapper(cfg => cfg.AddProfile<RecipeProfile>());
        }
    }
}