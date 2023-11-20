using System.Collections.Concurrent;
using nFirewall.Application.Abstractions;
using nFirewall.Application.DataProcessors.Models;
using nFirewall.Domain.Models;

// ReSharper disable once UnusedParameter.Local

namespace nFirewall.Application.DataProcessors;

public class CalculateSlowestUrlRequest : IDataProcessor, IReportContainer
{
    private static readonly ConcurrentDictionary<string, CalculateSlowestUrlData> UrlRequestTime = new();

    public string Name => "SlowUrlRequest";

    public Task Process(RequestData requestData, CancellationToken cancellationToken)
    {
        var timeTaken = requestData.FinishTime - requestData.StartTime;
        if (new TimeSpan(timeTaken).TotalSeconds <= 1)
        {
            return Task.CompletedTask;
        }

        UrlRequestTime.AddOrUpdate(requestData.Path,
            new CalculateSlowestUrlData(),
            (key, lastTimeData) => lastTimeData.Set(timeTaken));

        if (UrlRequestTime.Count > 10000)
        {
            CleanList();
        }

        return Task.CompletedTask;
    }

    public Task<object> GetData()
    {
        var clonedData = UrlRequestTime.ToList();
        object data = clonedData
            .OrderByDescending(d => d.Value.Duration)
            .Take(100)
            .Select(d => new
            {
                Url = d.Key,
                Duration = new TimeSpan(d.Value.Duration).ToString(),
                Time = new DateTime(d.Value.Time)
            })
            .ToList();

        return Task.FromResult(data);
    }

    private static void CleanList()
    {
        var clonedData = UrlRequestTime.ToList();
        var itemForRemove = clonedData
            .OrderBy(d => d.Value)
            .Take(clonedData.Count - 5000)
            .Select(d => d.Key)
            .ToList();

        foreach (var item in itemForRemove)
        {
            UrlRequestTime.TryRemove(item, out _);
        }
    }
}