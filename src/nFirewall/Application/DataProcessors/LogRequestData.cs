using Microsoft.Extensions.Logging;
using nFirewall.Domain.Models;
using nFirewall.Domain.Shared;

namespace nFirewall.Application.DataProcessors;

public class LogRequestData : IDataProcessor
{
    private readonly ILogger<LogRequestData> _logger;

    public LogRequestData(ILogger<LogRequestData> logger)
    {
        _logger = logger;
    }

    public string Name => "Log";

    public Task Process(RequestData requestData, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(requestData.StartTime);
        var endDate = new DateTime(requestData.FinishTime);
        var isIpV6 = requestData.Ip > int.MaxValue || requestData.Ip == 0x0000000000000001;
        var ip = IpAddressHelper.ConvertFromNumberToIpAddressString(requestData.Ip, isIpV6);

        _logger.LogInformation(
            "request {RequestDataTraceIdentifier}, IP:{Ip}, started={StartDate}, end={EndDate}, contentType={RequestDataContentType}",
            requestData.TraceIdentifier, ip, startDate, endDate, requestData.ContentType);

        return Task.CompletedTask;
    }

    public Task<object> GetData()
    {
        return Task.FromResult((object)string.Empty);
    }
}