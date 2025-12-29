namespace Orchestrator.Core.Ports.Out;

public interface ICustomerClient
{
    Task<bool> ExistsAsync(string customerId, CancellationToken ct);
}