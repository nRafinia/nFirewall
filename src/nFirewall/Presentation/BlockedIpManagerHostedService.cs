using Microsoft.Extensions.Hosting;
using nFirewall.Application;
using nFirewall.Application.Services;

namespace nFirewall.Presentation;

public class BlockedIpManagerHostedServiceService : IHostedService
{
    private readonly IBlockedIpManager _blockedIpManager;

    public BlockedIpManagerHostedServiceService(IBlockedIpManager blockedIpManager)
    {
        _blockedIpManager = blockedIpManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _= _blockedIpManager.StartProcessBannedAddresses(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _= _blockedIpManager.StopProcessBannedAddresses(cancellationToken);
        return Task.CompletedTask;
    }

   
}