namespace nFirewall.Domain.Models.AddressRanges;

public class BlackListAddressRange : AddressRangeList
{
    public BlackListAddressRange(IEnumerable<string> addressRanges) : base(addressRanges)
    {
    }
}