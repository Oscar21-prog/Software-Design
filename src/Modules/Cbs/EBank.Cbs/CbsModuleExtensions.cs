using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBank.Cbs;

public static class CbsModuleExtensions
{
    public static IServiceCollection AddCbsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Registrar DbContext (esquema 'cbs'), servicios y consumidores de eventos
        return services;
    }
}
