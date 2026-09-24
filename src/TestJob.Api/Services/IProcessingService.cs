using TestJob.Api.Models;

namespace TestJob.Api.Services;

public interface IProcessingService
{
    Task<ProcessResponse> ProcessAsync(ProcessRequest request, CancellationToken ct = default);
}
