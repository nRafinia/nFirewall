using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using nFirewall.Application.Services;
using nFirewall.Domain.Models;
using nFirewall.Domain.Models.AddressRanges;
using nFirewall.Domain.Shared;

namespace nFirewall.Presentation.Middlewares;

public class LogRequestsMiddleware
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<LogRequestsMiddleware> _logger;
    private readonly WhiteListAddressRange _whiteListAddress;

    private readonly RequestDelegate _next;

    public LogRequestsMiddleware(RequestDelegate next, IQueueManager queueManager,
        ILogger<LogRequestsMiddleware> logger, WhiteListAddressRange whiteListAddress)
    {
        _next = next;
        _queueManager = queueManager;
        _logger = logger;
        _whiteListAddress = whiteListAddress;
    }

    public async Task Invoke(HttpContext context)
    {
        var ip = IPAddress.Parse("0.0.0.0");
        if (context.Connection.RemoteIpAddress is not null)
        {
            ip = context.Connection.RemoteIpAddress.IsIPv4MappedToIPv6
                ? context.Connection.RemoteIpAddress.MapToIPv4()
                : context.Connection.RemoteIpAddress;
        }

        if (_whiteListAddress.IsIpInList(ip))
        {
            await _next(context);
            return;
        }

        var startTime = DateTime.Now.Ticks;
        var traceIdentifier = context.TraceIdentifier;
        var path = context.Request.Path.ToString();
        var nameIdentifier = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var requestData = new RequestData(traceIdentifier, ip.ConvertFromIpAddressToNumber(), startTime, path,
            nameIdentifier);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogError(ex, ex.Message);
            ProcessFinishedRequest(requestData, context, ex);
            throw;
        }

        ProcessFinishedRequest(requestData, context, default);
    }

    private void ProcessFinishedRequest(RequestData requestData, HttpContext context, Exception? ex)
    {
        var finishTime = DateTime.Now.Ticks;
        var statusCode = ex is not null ? StatusCodes.Status500InternalServerError : context.Response.StatusCode;
        var contentType = context.Response.ContentType ?? string.Empty;

        requestData.SetFinishData(statusCode, contentType, finishTime, ex?.Message);

        _queueManager.EnqueueRequest(requestData);
    }


}