using System.Collections.Concurrent;
using System.Numerics;
using nFirewall.Application.Abstractions;
using nFirewall.Domain.Models;
using nFirewall.Domain.Shared;

// ReSharper disable once UnusedParameter.Local

namespace nFirewall.Application.DataProcessors;

public class CalculateTotalIpRequest : IDataProcessor, IReportContainer
{
    private static readonly ConcurrentDictionary<BigInteger, long> IpRequestTime = new();

    public string Name => "TotalIPRequest";

    public Task Process(RequestData requestData, CancellationToken cancellationToken)
    {
        IpRequestTime.AddOrUpdate(requestData.Ip, 1, (key, currentCount) => currentCount + 1);

        if (IpRequestTime.Count > 10000)
        {
            CleanList();
        }

        return Task.CompletedTask;
    }

    public Task<object> GetData()
    {
        var clonedData = IpRequestTime.ToList();
        object data = clonedData
            .OrderByDescending(d => d.Value)
            .Take(100)
            .Select(d => new
            {
                IP = IpAddressHelper.ConvertFromNumberToIpAddressString(d.Key), 
                Count = d.Value
            })
            .ToList();

        return Task.FromResult(data);
    }

    private static void CleanList()
    {
        var clonedData = IpRequestTime.ToList();
        var itemForRemove = clonedData
            .OrderBy(d => d.Value)
            .Take(clonedData.Count - 5000)
            .Select(d => d.Key)
            .ToList();

        foreach (var item in itemForRemove)
        {
            IpRequestTime.TryRemove(item, out _);
        }
    }
}