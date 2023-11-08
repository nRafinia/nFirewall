using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace nFirewall.Domain.Shared;

public static class IpAddressHelper
{
    public static BigInteger ConvertFromIpAddressToNumber(string ipAddress)
    {
        var address = IPAddress.Parse(ipAddress);
        return ConvertFromIpAddressToNumber(address);
    }

    public static BigInteger ConvertFromIpAddressToNumber(this IPAddress ipAddress)
    {
        var bytes = ipAddress.GetAddressBytes();
        BigInteger result = 0;

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork) //IPv4
        {
            Array.Reverse(bytes);
            result = new BigInteger(bytes);
        }
        else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6) //IPv6
        {
            Array.Reverse(bytes);
            result = new BigInteger(bytes);
        }

        return result;
    }

    public static string ConvertFromNumberToIpAddress(BigInteger ipAddressValue, bool isIPv6 = false)
    {
        if (ipAddressValue == 2130706433)
        {
            // IPv4 localhost address
            return IPAddress.Loopback.ToString();
        }
        else if (ipAddressValue == 0x0000000000000001)
        {
            // IPv6 localhost address
            return IPAddress.IPv6Loopback.ToString();
        }
    
        byte[] bytes;
        if (isIPv6)
        {
            bytes = ipAddressValue.ToByteArray();
            Array.Reverse(bytes);
            return new IPAddress(bytes, 0).ToString();
        }
        else
        {
            bytes = ipAddressValue.ToByteArray();
            Array.Resize(ref bytes, 4);
            Array.Reverse(bytes);
            return new IPAddress(bytes).ToString();
        }
    }

}