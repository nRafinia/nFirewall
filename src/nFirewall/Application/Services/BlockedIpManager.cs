using System.Collections.Concurrent;
using System.Net;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nFirewall.Application.Abstractions;
using nFirewall.Domain.Shared;
using nFirewall.Persistence;

namespace nFirewall.Application.Services;

public class BlockedIpManager : IBlockedIpManager, IReportContainer
{
    private static ConcurrentDictionary<BigInteger, long>? _bannedAddresses;
    
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BlockedIpManager> _logger;
    
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1));
    private bool _stopProcessData;

    public string Name => "BlockedIP";

    public BlockedIpManager(IServiceScopeFactory scopeFactory, ILogger<BlockedIpManager> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<bool> IsIpBlocked(IPAddress ipAddress)
    {
        var requestAddress = ipAddress.ConvertFromIpAddressToNumber();
        return await IsIpBlocked(requestAddress);
    }

    public async Task<bool> IsIpBlocked(BigInteger ipAddress)
    {
        if (_bannedAddresses is null)
        {
            await LoadBannedAddresses();
        }

        if (_bannedAddresses is null)
        {
            return false;
        }

        return _bannedAddresses.TryGetValue(ipAddress, out var expireDate) && expireDate > DateTime.Now.Ticks;
    }

    public async Task BlockIp(IPAddress ipAddress, DateTime? expireDate = null)
    {
        var requestAddress = ipAddress.ConvertFromIpAddressToNumber();
        await BlockIp(requestAddress, expireDate);
    }

    public async Task BlockIp(BigInteger ipAddress, DateTime? expireDate = null)
    {
        if (_bannedAddresses is null)
        {
            await LoadBannedAddresses();
        }

        if (_bannedAddresses is null)
        {
            return;
        }
        
        _bannedAddresses.TryAdd(ipAddress, (expireDate ?? DateTime.MaxValue).Ticks);
        _logger.LogInformation("Blocked IP {IpAddress}", IpAddressHelper.ConvertFromNumberToIpAddress(ipAddress));
    }

    public Task UnblockIp(IPAddress ipAddress)
    {
        var requestAddress = ipAddress.ConvertFromIpAddressToNumber();
        return UnblockIp(requestAddress);
    }

    public async Task UnblockIp(BigInteger ipAddress)
    {
        if (_bannedAddresses is null)
        {
            await LoadBannedAddresses();
        }

        if (_bannedAddresses is null)
        {
            return;
        }

        _bannedAddresses.TryRemove(ipAddress, out _);
        _logger.LogDebug("Unblocked IP {IpAddress}", IpAddressHelper.ConvertFromNumberToIpAddress(ipAddress));
    }

    public async Task<object> GetData()
    {
        if (_bannedAddresses is null)
        {
            await LoadBannedAddresses();
        }

        if (_bannedAddresses is null)
        {
            return Array.Empty<string>();
        }
        
        object result = _bannedAddresses.Keys.Select(b => IpAddressHelper.ConvertFromNumberToIpAddress(b));

        return result;
    }

    #region Background checks

    public async Task StartProcessBannedAddresses(CancellationToken cancellationToken)
    {
        while (!_stopProcessData || !cancellationToken.IsCancellationRequested)
        {
            await _timer.WaitForNextTickAsync(cancellationToken);

            RemoveExpiredAddresses();
        }
    }

    public Task StopProcessBannedAddresses(CancellationToken cancellationToken)
    {
        _stopProcessData = true;
        return Task.CompletedTask;
    }

    #endregion

    #region Private methods

    private async Task LoadBannedAddresses()
    {
        var now = DateTime.Now;
        using var scope = _scopeFactory.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
        var bannedAddresses = await dbContext!.BannedAddresses
            .Where(b => b.ExpireDate == null || b.ExpireDate > now)
            .Select(b => new { b.Ip, b.ExpireDate })
            .ToListAsync();

        _bannedAddresses = new ConcurrentDictionary<BigInteger, long>();
        foreach (var bannedAddress in bannedAddresses)
        {
            _bannedAddresses.TryAdd(IpAddressHelper.ConvertFromIpAddressToNumber(bannedAddress.Ip),
                (bannedAddress.ExpireDate ?? DateTime.MaxValue).Ticks);
            _logger.LogInformation("Default blocked IP {IpAddress}", bannedAddress.Ip);
        }
    }

    private void RemoveExpiredAddresses()
    {
        if (_bannedAddresses is null)
        {
            return;
        }
        
        var now = DateTime.Now;
        foreach (var bannedAddress in _bannedAddresses)
        {
            if (bannedAddress.Value >= now.Ticks)
            {
                continue;
            }

            var ip = bannedAddress.Key;
            _bannedAddresses.TryRemove(ip, out _);
            _logger.LogDebug("Removed blocked IP {IpAddress}", IpAddressHelper.ConvertFromNumberToIpAddress(ip));
        }
    }

    #endregion
}