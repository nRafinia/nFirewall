﻿using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using nFirewall.Application;
using nFirewall.Application.Services;
using nFirewall.Domain.Models;
using nFirewall.Domain.Shared;

namespace nFirewall.Presentation;

public class LogRequestsMiddleware
{
    private readonly IQueueManager _queueManager;

    private readonly RequestDelegate _next;
    private readonly ConcurrentDictionary<string, RequestData> _currentRequests;

    public LogRequestsMiddleware(RequestDelegate next, IQueueManager queueManager)
    {
        _next = next;
        _currentRequests = new ConcurrentDictionary<string, RequestData>();
        _queueManager = queueManager;
    }

    public async Task Invoke(HttpContext context)
    {
        var startTime = DateTime.Now.Ticks;
        var traceIdentifier = context.TraceIdentifier;
        var path = context.Request.Path.ToString();
        var nameIdentifier = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var requestData = new RequestData(traceIdentifier,
            context.Connection.RemoteIpAddress?.ConvertFromIpAddressToNumber() ?? 0,
            startTime, path, nameIdentifier);
        _currentRequests.TryAdd(traceIdentifier, requestData);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
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