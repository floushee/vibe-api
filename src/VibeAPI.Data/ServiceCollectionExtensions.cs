using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VibeAPI.Application.Common;

namespace VibeAPI.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVibeData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("VibeDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'VibeDb' is not configured.");
        }

        services.AddDbContextPool<VibeDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IVibeDbContext>(sp => sp.GetRequiredService<VibeDbContext>());

        return services;
    }
}
