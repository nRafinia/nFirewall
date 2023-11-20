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

    public GetFirewallDataMiddleware(RequestDelegate next, IEnumerable<IReportContainer> dataProcessors,
        FirewallSetting setting)
    {
        _next = next;
        _dataProcessors = dataProcessors;
        _setting = setting;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!string.Equals(context.Request.Path.ToString(), $"/{_setting.ReportPath}",
                StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        await SendReport(context);
    }

    private async Task SendReport(HttpContext context)
    {
        if (!context.Request.Query.TryGetValue("type", out var type))
        {
            await SendCommands(context);
            return;
        }

        await SendProviderData(context, type.ToString());
    }

    private async Task SendCommands(HttpContext context)
    {
        var dataProcessors = _dataProcessors
            .Select(c => c.Name)
            .ToList();

        context.Response.Clear();
        context.Response.Headers.Clear();
        context.Response.ContentType = "text/html";
        var content = string.Join("", dataProcessors.Select(d => $"<a href=\"?type={d}\">{d}</a><br/>"));
        var responseHtml = $"<html><body>{content}</body></html>";
        await context.Response.WriteAsync(responseHtml);
    }

    private async Task SendProviderData(HttpContext context, string type)
    {
        var dataProcessor = _dataProcessors.FirstOrDefault(d =>
            string.Equals(d.Name, type, StringComparison.CurrentCultureIgnoreCase));

        if (dataProcessor is null)
        {
            return;
        }

        var result = await dataProcessor.GetData();

        context.Response.Clear();
        context.Response.Headers.Clear();
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
}