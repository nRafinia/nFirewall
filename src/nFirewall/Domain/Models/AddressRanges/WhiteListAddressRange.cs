namespace nFirewall.Domain.Models.AddressRanges;

public class WhiteListAddressRange : AddressRangeList
{
    public WhiteListAddressRange(IEnumerable<string> addressRanges) : base(addressRanges)
    {
    }
}