using System.Net;
using System.Numerics;

namespace nFirewall.Application.Services;

public interface IBlockedIpManager 
{
    Task<bool> IsIpBlocked(IPAddress ipAddress);
    Task<bool> IsIpBlocked(BigInteger ipAddress);
    Task BlockIp(IPAddress ipAddress, DateTime? expireDate = null);
    Task BlockIp(BigInteger ipAddress, DateTime? expireDate = null);
    Task UnblockIp(IPAddress ipAddress);
    Task UnblockIp(BigInteger ipAddress);
    Task StartProcessBannedAddresses(CancellationToken cancellationToken);
    Task StopProcessBannedAddresses(CancellationToken cancellationToken);
}