using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBank.Crm;

public static class CrmModuleExtensions
{
    public static IServiceCollection AddCrmModule(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Registrar DbContext (esquema 'crm'), servicios y consumidores de eventos
        return services;
    }
}
