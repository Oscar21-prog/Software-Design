using EBank.BuildingBlocks.Events;
using EBank.BuildingBlocks.Time;
using Microsoft.Extensions.DependencyInjection;

namespace EBank.BuildingBlocks;

public static class BuildingBlocksExtensions
{
    public static IServiceCollection AddBuildingBlocks(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        return services;
    }
}
