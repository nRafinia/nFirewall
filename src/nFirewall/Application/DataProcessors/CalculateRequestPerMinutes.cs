using System.Collections.Concurrent;
using System.Numerics;
using nFirewall.Application.Abstractions;
using nFirewall.Application.DataProcessors.Models;
using nFirewall.Domain.Models;
using nFirewall.Domain.Shared;

// ReSharper disable once UnusedParameter.Local

namespace nFirewall.Application.DataProcessors;

public class CalculateRequestPerMinutes : IDataProcessor, IReportContainer
{
    private static readonly ConcurrentDictionary<int, long> IpRequestTime = new();
    private static int _hour = DateTime.Now.Hour;

    public string Name => "RequestPerMinute";

    public Task Process(RequestData requestData, CancellationToken cancellationToken)
    {
        if (DateTime.Now.Hour != _hour)
        {
            IpRequestTime.Clear();
            _hour = DateTime.Now.Hour;
        }

        IpRequestTime.AddOrUpdate(DateTime.Now.Minute,
            1,
            (key, current) => current + 1);

        return Task.CompletedTask;
    }

    public Task<object> GetData()
    {
        var clonedData = IpRequestTime.ToList();
        object data = clonedData
            .OrderBy(d => d.Key)
            .Select(d => new
            {
                Hour = _hour,
                Minutes = d.Key,
                Total = d.Value
            })
            .ToList();

        return Task.FromResult(data);
    }
}