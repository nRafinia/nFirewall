using nFirewall.Domain.Models;

namespace nFirewall.Application.Services;

public interface IQueueManager
{
    void EnqueueRequest(RequestData requestData);
    RequestData? DequeueRequest();
}