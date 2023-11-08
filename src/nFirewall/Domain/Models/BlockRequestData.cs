using System.Net;

namespace nFirewall.Domain.Models;

public record BlockRequestData(bool MustBlock, HttpStatusCode HttpStatusCode = HttpStatusCode.TooManyRequests,
    string? Message = null)
{
    public static BlockRequestData Ok()
    {
        return new BlockRequestData(false);
    }

    public static BlockRequestData Block(HttpStatusCode httpStatusCode = HttpStatusCode.TooManyRequests,
        string? message = null)
    {
        return new BlockRequestData(true, httpStatusCode, message);
    }
}