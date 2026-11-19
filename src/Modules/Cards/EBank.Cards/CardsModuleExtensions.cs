using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBank.Cards;

public static class CardsModuleExtensions
{
    public static IServiceCollection AddCardsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Registrar DbContext (esquema 'cards'), servicios y consumidores de eventos
        return services;
    }
}
