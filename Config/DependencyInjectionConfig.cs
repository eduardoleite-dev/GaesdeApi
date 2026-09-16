using GaesdeApi.Services;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Config;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddDependencyInjectionConfig(this IServiceCollection services)
    {
        services.AddScoped<IMongoTestService, MongoTestService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}