using Microsoft.AspNetCore.Http;
using nFirewall.Application.Services;
using nFirewall.Domain.Models;
using nFirewall.Domain.Models.AddressRanges;

namespace nFirewall.Application.BlockModules;

public class BlockIpModule : IBlockModule
{
    private readonly BlackListAddressRange _blackListAddressRange;

    public BlockIpModule(BlackListAddressRange blackListAddressRange)
    {
        _blackListAddressRange = blackListAddressRange;
    }

    public Task<BlockRequestData> CheckRequest(HttpContext context)
    {
        if (context.Connection.RemoteIpAddress is null)
        {
            return Task.FromResult(BlockRequestData.Ok());
        }

        var result= _blackListAddressRange.IsIpInList(context.Connection.RemoteIpAddress)
            ? BlockRequestData.Block(message: $"Your IP {context.Connection.RemoteIpAddress} is banned")
            : BlockRequestData.Ok();

        return Task.FromResult(result);
    }

}