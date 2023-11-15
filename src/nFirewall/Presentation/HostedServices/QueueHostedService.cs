using Microsoft.Extensions.Hosting;
using nFirewall.Application.Services;

namespace nFirewall.Presentation.HostedServices;

public class QueueHostedService : IHostedService
{
    private readonly IQueueProcessor _queueProcessor;

    public QueueHostedService(IQueueProcessor queueProcessor)
    {
        _queueProcessor = queueProcessor;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _= _queueProcessor.StartProcessQueue(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _= _queueProcessor.StopProcessQueue(cancellationToken);
        return Task.CompletedTask;
    }

   
}