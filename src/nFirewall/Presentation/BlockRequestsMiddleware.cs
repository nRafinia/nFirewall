using Microsoft.AspNetCore.Http;
using nFirewall.Application.BlockModules;

namespace nFirewall.Presentation;

public class BlockRequestsMiddleware
{
    private readonly IEnumerable<IBlockModule> _blockModules;
    private readonly RequestDelegate _next;

    public BlockRequestsMiddleware(RequestDelegate next, IEnumerable<IBlockModule> blockModules)
    {
        _next = next;
        _blockModules = blockModules;
    }

    public async Task Invoke(HttpContext context)
    {
        if (string.Equals(context.Request.Path.ToString(), $"/{Consts.GetReportPath}", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        foreach (var blockModule in _blockModules)
        {
            var blockResponse = await blockModule.CheckRequest(context);
            if (!blockResponse.MustBlock)
            {
                continue;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)blockResponse.HttpStatusCode;
            context.Response.ContentType = "text/plain";
            if (!string.IsNullOrWhiteSpace(blockResponse.Message))
            {
                await context.Response.WriteAsync(blockResponse.Message);
            }

            return;
        }

        await _next(context);
    }
}