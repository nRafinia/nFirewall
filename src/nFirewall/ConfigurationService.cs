using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nFirewall.Application.Abstractions;
using nFirewall.Application.BlockModules;
using nFirewall.Application.DataProcessors;
using nFirewall.Application.Services;
using nFirewall.Domain.Models;
using nFirewall.Domain.Models.AddressRanges;
using nFirewall.Domain.Shared;
using nFirewall.Presentation.HostedServices;
using nFirewall.Presentation.Middlewares;

namespace nFirewall;

public static class ConfigurationService
{
    public static IServiceCollection AddFirewallService(this IServiceCollection services,
        IConfiguration configuration, Func<FirewallSetting>? settings = null)
    {
        services.AddHostedService<QueueHostedService>();
        services.AddSingleton<IQueueManager, QueueManager>();
        services.AddSingleton<IQueueProcessor, QueueProcessor>();

        AddDataProcessors(services);
        AddBlockModules(services);
        AddReportContainer(services);

        LoadSettings(services, configuration, settings);

        //services.AddMemoryCache();

        return services;
    }

    public static IApplicationBuilder UseFirewall(this WebApplication app, string reportPath = Consts.GetReportPath)
    {
        app.UseMiddleware<BlockRequestsMiddleware>();
        app.UseMiddleware<LogRequestsMiddleware>();
        app.UseMiddleware<GetFirewallDataMiddleware>();

        return app;
    }

    #region private methods

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

    private static void LoadSettings(IServiceCollection services, IConfiguration configuration,
        Func<FirewallSetting>? settings = null)
    {
        var firewallSetting = settings?.Invoke() ??
                              configuration.GetSection("FirewallSetting").Get<FirewallSetting>() ??
                              new FirewallSetting();
        services.AddSingleton(firewallSetting);

        var whiteList = new WhiteListAddressRange(firewallSetting.WhiteList);
        services.AddSingleton(whiteList);

        var blackList = new BlackListAddressRange(firewallSetting.BlackList);
        services.AddSingleton(blackList);
    }

    #endregion
}