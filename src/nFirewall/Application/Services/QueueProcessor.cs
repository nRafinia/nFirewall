using Microsoft.Extensions.Logging;
using nFirewall.Application.DataProcessors;
using nFirewall.Domain.Models;
using nFirewall.Domain.Shared;

namespace nFirewall.Application.Services;

public class QueueProcessor : IQueueProcessor
{
    private readonly IQueueManager _queueManager;
    private readonly IEnumerable<IDataProcessor> _dataProcessors;
    private readonly ILogger<QueueProcessor> _logger;

    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1));
    private readonly RoundRobinExecutor _executor;
    private bool _stopProcessData;

    public QueueProcessor(IQueueManager queueManager, IEnumerable<IDataProcessor> dataProcessors,
        ILogger<QueueProcessor> logger, FirewallSetting setting)
    {
        _queueManager = queueManager;
        _dataProcessors = dataProcessors;
        _logger = logger;
        _executor = new RoundRobinExecutor(setting.ThreadCount);
    }

    #region Private methods

    public async Task StartProcessQueue(CancellationToken cancellationToken)
    {
        while (!_stopProcessData || !cancellationToken.IsCancellationRequested)
        {
            var requestData = _queueManager.DequeueRequest();
            while (requestData is not null)
            {
                try
                {
                    var request = requestData.Value;

                    async void Action() => await ProcessData(request, cancellationToken);
                    _executor.Execute(Action);
                }
                catch (Exception ex)
                {
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    _logger.LogError(ex, ex.Message);
                }

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
        try
        {
            foreach (var dataProcessor in _dataProcessors)
            {
                await dataProcessor.Process(requestData, cancellationToken);
            }
        }
        finally
        {
            GC.Collect();
        }
    }

    #endregion
}