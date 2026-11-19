using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBank.Kyc;

public static class KycModuleExtensions
{
    public static IServiceCollection AddKycModule(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Registrar DbContext (esquema 'kyc'), servicios y consumidores de eventos
        return services;
    }
}
