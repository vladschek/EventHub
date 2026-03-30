using Azure.Messaging.ServiceBus;
using EventHub.Core.Abstractions;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Messaging;
using EventHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventHubInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var sql = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(sql));

        var disablePublishing = bool.TryParse(configuration["ServiceBus:DisablePublishing"], out var noBus) && noBus;
        if (disablePublishing)
            services.AddSingleton<IEventPublisher, NoOpEventPublisher>();
        else
        {
            var bus = configuration["ServiceBus:ConnectionString"]
                ?? configuration["Values:ServiceBusConnection"];
            if (string.IsNullOrWhiteSpace(bus))
                throw new InvalidOperationException(
                    "Service Bus connection string is required. Set ServiceBus:ConnectionString (or Values:ServiceBusConnection for Functions local.settings), or set ServiceBus:DisablePublishing to true for local-only API runs.");

            services.AddSingleton(_ => new ServiceBusClient(bus));
            services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
        }

        services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
}
