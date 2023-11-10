using System.Numerics;
using Microsoft.AspNetCore.Http;

namespace nFirewall.Domain.Models;

public struct RequestData
{
    public string TraceIdentifier { get; }
    public BigInteger Ip { get; }
    public long StartTime { get; }
    public string Path { get; }
    public string NameIdentifier { get; }
    public ConnectionInfo Connection { get; }
    public int StatusCode { get; private set; } = 0;
    public string ContentType { get; private set; } = string.Empty;
    public long FinishTime { get; private set; } = 0;
    public string? ExceptionMessage { get; private set; } = string.Empty;

    public RequestData(string traceIdentifier, BigInteger ip, long startTime, string path, string nameIdentifier,
        ConnectionInfo connection)
    {
        TraceIdentifier = traceIdentifier;
        StartTime = startTime;
        Path = path;
        NameIdentifier = nameIdentifier;
        Ip = ip;
        Connection = connection;
    }

    public void SetFinishData(int statusCode, string contentType, long finishTime, string? exceptionMessage)
    {
        StatusCode = statusCode;
        ContentType = contentType;
        FinishTime = finishTime;
        ExceptionMessage = exceptionMessage;
    }

    public bool IsFinished => FinishTime != 0;
}