using System.Text.Json;
using Microsoft.AspNetCore.Http;
using nFirewall.Application.Abstractions;

namespace nFirewall.Presentation;

public class GetFirewallDataMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IReportContainer> _dataProcessors;

    public GetFirewallDataMiddleware(RequestDelegate next, IEnumerable<IReportContainer> dataProcessors)
    {
        _next = next;
        _dataProcessors = dataProcessors;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!string.Equals(context.Request.Path.ToString(), $"/{Consts.GetReportPath}", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        await SendReport(context);
    }

    private async Task SendReport(HttpContext content)
    {
        if (!content.Request.Query.TryGetValue("type", out var type))
        {
            return;
        }
        
        var dataProcessor = _dataProcessors.FirstOrDefault(d =>
            string.Equals(d.Name, type, StringComparison.CurrentCultureIgnoreCase));

        if (dataProcessor is null)
        {
            return;
        }

        var result = await dataProcessor.GetData();
        
        content.Response.Clear();
        content.Response.Headers.Clear();
        content.Response.ContentType = "application/json";
        await content.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
}