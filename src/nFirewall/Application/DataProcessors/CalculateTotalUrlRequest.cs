using System.Collections.Concurrent;
using nFirewall.Application.Abstractions;
using nFirewall.Domain.Models;

// ReSharper disable once UnusedParameter.Local

namespace nFirewall.Application.DataProcessors;

public class CalculateTotalUrlRequest : IDataProcessor, IReportContainer
{
    private static readonly ConcurrentDictionary<string, long> UrlRequestCounts = new();

    public string Name => "TotalUrlRequest";

    public Task Process(RequestData requestData, CancellationToken cancellationToken)
    {
        UrlRequestCounts.AddOrUpdate(requestData.Path, 1, (key, currentCount) => currentCount + 1);

        if (UrlRequestCounts.Count > 10000)
        {
            CleanList();
        }

        return Task.CompletedTask;
    }

    public Task<object> GetData()
    {
        var clonedData = UrlRequestCounts.ToList();
        object data = clonedData
            .OrderByDescending(d => d.Value)
            .Take(100)
            .Select(d => new { Url = d.Key, Count = d.Value })
            .ToList();

        return Task.FromResult(data);
    }

    private static void CleanList()
    {
        var clonedData = UrlRequestCounts.ToList();
        var itemForRemove = clonedData
            .OrderBy(d => d.Value)
            .Take(clonedData.Count - 5000)
            .Select(d => d.Key)
            .ToList();

        foreach (var item in itemForRemove)
        {
            UrlRequestCounts.TryRemove(item, out _);
        }
    }
}