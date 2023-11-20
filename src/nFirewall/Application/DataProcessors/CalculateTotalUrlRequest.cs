using System.Collections.Concurrent;
using nFirewall.Application.Abstractions;
using nFirewall.Application.DataProcessors.Models;
using nFirewall.Domain.Models;

// ReSharper disable once UnusedParameter.Local

namespace nFirewall.Application.DataProcessors;

public class CalculateTotalUrlRequest : IDataProcessor, IReportContainer
{
    private static readonly ConcurrentDictionary<string, CalculateTotalUrlData> UrlRequestCounts = new();

    public string Name => "TotalUrlRequest";

    public Task Process(RequestData requestData, CancellationToken cancellationToken)
    {
        UrlRequestCounts.AddOrUpdate(requestData.Path,
            new CalculateTotalUrlData(),
            (key, current) => current.Increase());

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
            .OrderByDescending(d => d.Value.Total)
            .Take(100)
            .Select(d => new
            {
                Url = d.Key,
                Start = new DateTime(d.Value.Start),
                End = new DateTime(d.Value.End),
                d.Value.Total
            })
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