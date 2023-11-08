using Microsoft.AspNetCore.Http;
using nFirewall.Domain.Models;

namespace nFirewall.Application.BlockModules;

public interface IBlockModule
{
    Task<BlockRequestData> CheckRequest(HttpContext context);
}