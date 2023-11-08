using nFirewall.Application.DataProcessors;
using nFirewall.Domain.Models;

namespace nFirewall.Application.Services;

public class QueueProcessor : IQueueProcessor
{
    private readonly IQueueManager _queueManager;
    private readonly IEnumerable<IDataProcessor> _dataProcessors;

    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1));
    private bool _stopProcessData;

    public QueueProcessor(IQueueManager queueManager, IEnumerable<IDataProcessor> dataProcessors)
    {
        _queueManager = queueManager;
        _dataProcessors = dataProcessors;
    }

    #region Private methods
    
    public async Task StartProcessQueue(CancellationToken cancellationToken)
    {
        while (!_stopProcessData || !cancellationToken.IsCancellationRequested)
        {
            var requestData = _queueManager.DequeueRequest();
            while (requestData is not null)
            {
                await ProcessData(requestData.Value, cancellationToken);

                requestData = _queueManager.DequeueRequest();
            }

            await _timer.WaitForNextTickAsync(cancellationToken);
        }
    }

    public Task StopProcessQueue(CancellationToken cancellationToken)
    {
        _stopProcessData = true;
        return Task.CompletedTask;
    }

    private async Task ProcessData(RequestData requestData, CancellationToken cancellationToken)
    {
        foreach (var dataProcessor in _dataProcessors)
        {
            await dataProcessor.Process(requestData, cancellationToken);
        }
    }

    #endregion
}