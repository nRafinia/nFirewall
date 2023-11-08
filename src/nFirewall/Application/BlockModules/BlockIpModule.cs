using Microsoft.AspNetCore.Http;
using nFirewall.Application.Services;
using nFirewall.Domain.Models;

namespace nFirewall.Application.BlockModules;

public class BlockIpModule : IBlockModule
{
    private readonly IBlockedIpManager _blockedIpManager;

    public BlockIpModule(IBlockedIpManager blockedIpManager)
    {
        _blockedIpManager = blockedIpManager;
    }

    public async Task<BlockRequestData> CheckRequest(HttpContext context)
    {
        if (context.Connection.RemoteIpAddress is null)
        {
            return BlockRequestData.Ok();
        }

        return await _blockedIpManager.IsIpBlocked(context.Connection.RemoteIpAddress)
            ? BlockRequestData.Block(message: $"Your IP {context.Connection.RemoteIpAddress} is banned")
            : BlockRequestData.Ok();
    }

}