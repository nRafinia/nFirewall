namespace nFirewall.Application.Services;

public interface IQueueProcessor
{
    Task StartProcessQueue(CancellationToken cancellationToken);
    Task StopProcessQueue(CancellationToken cancellationToken);

}