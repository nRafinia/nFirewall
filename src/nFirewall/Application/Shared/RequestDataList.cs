using System.Collections.Concurrent;
using System.Numerics;
using nFirewall.Domain.Models;

namespace nFirewall.Application.Shared;

public static class RequestDataList
{
    private static readonly ConcurrentBag<RequestData> Data = new();

    private static bool _stopProcessData;
    private static readonly PeriodicTimer RemoveTimer = new(TimeSpan.FromSeconds(1));

    static RequestDataList()
    {
        _ = StartProcessBannedAddresses();
    }

    public static RequestData? Get(string traceIdentifier) =>
        Data.FirstOrDefault(x => x.TraceIdentifier == traceIdentifier);

    public static IEnumerable<RequestData> Get(BigInteger ip) => Data.Where(x => x.Ip == ip);

    public static void Add(RequestData data)
    {
        Data.Add(data);
    }

    public static bool Remove(RequestData data) => Data.TryTake(out _);

    public static bool Remove(string traceIdentifier)
    {
        var data = Get(traceIdentifier);
        return data is not null && Remove(data.Value);
    }

    public static void SetFinishData(string traceIdentifier, int statusCode, string contentType, long finishTime,
        string? exceptionMessage)
    {
        var data = Get(traceIdentifier);
        data?.SetFinishData(statusCode, contentType, finishTime, exceptionMessage);
    }

    public static void Stop()
    {
        _stopProcessData = true;
    }


    private static async Task StartProcessBannedAddresses()
    {
        while (!_stopProcessData)
        {
            await RemoveTimer.WaitForNextTickAsync();

            try
            {
                RemoveExpiredItem();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private static void RemoveExpiredItem()
    {
        if (Data.IsEmpty)
        {
            return;
        }

        var now = DateTime.Now;
        foreach (var item in Data)
        {
            var startTime = new DateTime(item.StartTime);

            // Remove item if it's older than 15 minutes
            if ((now - startTime).TotalMinutes < 15)
            {
                continue;
            }

            var connection = item.Connection;
            Remove(item);
            connection.RequestClose();
        }
    }
}