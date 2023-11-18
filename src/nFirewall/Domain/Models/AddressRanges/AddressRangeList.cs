using System.Net;
using NetTools;

namespace nFirewall.Domain.Models.AddressRanges;

public abstract class AddressRangeList
{
    private List<IPAddressRange> AddressRanges { get; set; }

    protected AddressRangeList(IEnumerable<string> addressRanges)
    {
        AddressRanges = addressRanges.Select(IPAddressRange.Parse).ToList();
    }

    public bool IsIpInList(IPAddress ipAddress)
    {
        return AddressRanges.Any(addressRange => addressRange.Contains(ipAddress));
    }
    
    public IReadOnlyList<IPAddressRange> GetAddressRanges()
    {
        return AddressRanges.AsReadOnly();
    }
}