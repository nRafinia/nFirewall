using System.Collections.Concurrent;
using nFirewall.Domain.Models;

namespace nFirewall.Application.Services;

public class QueueManager : IQueueManager
{
    private static readonly ConcurrentQueue<RequestData> FinishedDataQueue = new();

    public void EnqueueRequest(RequestData requestData)
    {
        
        FinishedDataQueue.Enqueue(requestData);
    }

    public RequestData? DequeueRequest()
    {
        return FinishedDataQueue.TryDequeue(out var data)
            ? data
            : default(RequestData?);
    }
}