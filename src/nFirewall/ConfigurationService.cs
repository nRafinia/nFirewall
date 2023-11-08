using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nFirewall.Application.Abstractions;
using nFirewall.Application.BlockModules;
using nFirewall.Application.DataProcessors;
using nFirewall.Application.Services;
using nFirewall.Domain.Shared;
using nFirewall.Persistence;
using nFirewall.Presentation;

namespace nFirewall;

public static class ConfigurationService
{
    public static IServiceCollection AddFirewallService(this IServiceCollection services,
        IConfiguration configuration)
    {
        AddDbContext(services);

        services.AddHostedService<QueueHostedService>();
        services.AddHostedService<BlockedIpManagerHostedServiceService>();
        services.AddSingleton<IQueueManager, QueueManager>();
        services.AddSingleton<IQueueProcessor, QueueProcessor>();
        services.AddSingleton<IBlockedIpManager, BlockedIpManager>();

        AddDataProcessors(services);
        AddBlockModules(services);
        AddReportContainer(services);

        services.AddMemoryCache();

        return services;
    }

    public static IApplicationBuilder UseFirewall(this WebApplication app, string reportPath = Consts.GetReportPath)
    {
        app.UseMiddleware<BlockRequestsMiddleware>();
        app.UseMiddleware<LogRequestsMiddleware>();

        app.MapGet($"/{reportPath}",
            async ([FromServices] IEnumerable<IReportContainer> dataProcessors, string? type) =>
            await GetFirewallData(dataProcessors, type));


        return app;
    }

    #region private methods

    private static void AddDbContext(IServiceCollection services)
    {
        var dbPath = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "nFirewall.db");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath}",
                builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            );
        });
    }

    private static void AddDataProcessors(IServiceCollection services)
    {
        var processors = Common.GetImplementedInterfaceOf<IDataProcessor>(typeof(ConfigurationService).Assembly);
        foreach (var dataProcessor in processors)
        {
            services.AddSingleton(typeof(IDataProcessor), dataProcessor);
        }
    }

    private static void AddBlockModules(IServiceCollection services)
    {
        var blockModules = Common.GetImplementedInterfaceOf<IBlockModule>(typeof(ConfigurationService).Assembly);
        foreach (var module in blockModules)
        {
            services.AddSingleton(typeof(IBlockModule), module);
        }
    }

    private static void AddReportContainer(IServiceCollection services)
    {
        var blockModules = Common.GetImplementedInterfaceOf<IReportContainer>(typeof(ConfigurationService).Assembly);
        foreach (var module in blockModules)
        {
            services.AddSingleton(typeof(IReportContainer), module);
        }
    }

    private static async Task<IResult> GetFirewallData([FromServices] IEnumerable<IReportContainer> dataProcessors,
        string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return Results.Empty;
        }

        var dataProcessor = dataProcessors.FirstOrDefault(d =>
            string.Equals(d.Name, type, StringComparison.CurrentCultureIgnoreCase));

        if (dataProcessor is null)
        {
            return Results.Empty;
        }

        var result = await dataProcessor.GetData();
        return Results.Ok(result);
    }

    #endregion
}