using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBank.Loans;

public static class LoansModuleExtensions
{
    public static IServiceCollection AddLoansModule(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Registrar DbContext (esquema 'loans'), servicios y consumidores de eventos
        return services;
    }
}
