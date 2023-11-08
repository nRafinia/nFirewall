namespace nFirewall.Application.Abstractions;

public interface IReportContainer
{
    string Name { get; }
    Task<object> GetData();
}