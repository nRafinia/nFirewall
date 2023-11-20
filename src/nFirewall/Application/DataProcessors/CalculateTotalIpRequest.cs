using System.Collections.Concurrent;
using System.Numerics;
using nFirewall.Application.Abstractions;
using nFirewall.Application.DataProcessors.Models;
using nFirewall.Domain.Models;
using nFirewall.Domain.Shared;

// ReSharper disable once UnusedParameter.Local

namespace nFirewall.Application.DataProcessors;

public class CalculateTotalIpRequest : IDataProcessor, IReportContainer
{
    private static readonly ConcurrentDictionary<BigInteger, CalculateTotalIpData> IpRequestTime = new();

    public string Name => "TotalIPRequest";

    public Task Process(RequestData requestData, CancellationToken cancellationToken)
    {
        IpRequestTime.AddOrUpdate(requestData.Ip, 
            new CalculateTotalIpData(), 
            (key, current) => current.Increase());

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
                Start = new DateTime(d.Value.Start),
                End = new DateTime(d.Value.End),
                d.Value.Total
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