using nFirewall.Domain.Models;

namespace nFirewall.Application.DataProcessors;

public class LogRequestData : IDataProcessor
{
    public string Name => "Log";

    public Task Process(RequestData requestData, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(requestData.StartTime);
        var endDate = new DateTime(requestData.FinishTime);

        Console.WriteLine(
            $"request {requestData.TraceIdentifier}, started={startDate}, end={endDate}, contentType={requestData.ContentType}");

        return Task.CompletedTask;
    }

    public Task<object> GetData()
    {
        return Task.FromResult((object)string.Empty);
    }
}