namespace nFirewall.Domain.Entities;

public class BannedAddress
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public string Ip { get; } = string.Empty;
    public bool Permanent { get; }
    public DateTime? ExpireDate { get; }

    /// <summary>
    /// For EF core
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private BannedAddress()
    {
    }

    public BannedAddress(string ip, bool permanent, DateTime? expireDate)
    {
        Ip = ip;
        Permanent = permanent;
        ExpireDate = expireDate;
    }
}