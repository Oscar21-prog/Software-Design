using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBank.Aml;

public static class AmlModuleExtensions
{
    public static IServiceCollection AddAmlModule(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Registrar DbContext (esquema 'aml'), servicios y consumidores de eventos
        return services;
    }
}
