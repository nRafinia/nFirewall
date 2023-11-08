using nFirewall.Domain.Models;

namespace nFirewall.Application.DataProcessors;

public interface IDataProcessor
{
    Task Process(RequestData requestData, CancellationToken cancellationToken);

}