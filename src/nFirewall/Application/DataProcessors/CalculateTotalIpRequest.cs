using System.Collections.Concurrent;
using nFirewall.Application.Abstractions;
using nFirewall.Domain.Models;
using nFirewall.Domain.Shared;

// ReSharper disable once UnusedParameter.Local

namespace nFirewall.Application.DataProcessors;

public class CalculateTotalIpRequest : IDataProcessor, IReportContainer
{
    private static readonly ConcurrentDictionary<string, long> IpRequestTime = new();

    public string Name => "TotalIPRequest";

    public Task Process(RequestData requestData, CancellationToken cancellationToken)
    {
        var isIpV6 = requestData.Ip > int.MaxValue || requestData.Ip == 0x0000000000000001;
        var ip = IpAddressHelper.ConvertFromNumberToIpAddress(requestData.Ip, isIpV6);
        IpRequestTime.AddOrUpdate(ip, 1, (key, currentCount) => currentCount + 1);

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
            .Select(d => new { Url = d.Key, Count = d.Value })
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