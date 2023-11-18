using System.Text.Json;
using Microsoft.AspNetCore.Http;
using nFirewall.Application.Abstractions;
using nFirewall.Domain.Models;

namespace nFirewall.Presentation.Middlewares;

public class GetFirewallDataMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IReportContainer> _dataProcessors;
    private readonly FirewallSetting _setting;

    public GetFirewallDataMiddleware(RequestDelegate next, IEnumerable<IReportContainer> dataProcessors, FirewallSetting setting)
    {
        _next = next;
        _dataProcessors = dataProcessors;
        _setting = setting;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!string.Equals(context.Request.Path.ToString(), $"/{_setting.ReportPath}", StringComparison.OrdinalIgnoreCase))
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