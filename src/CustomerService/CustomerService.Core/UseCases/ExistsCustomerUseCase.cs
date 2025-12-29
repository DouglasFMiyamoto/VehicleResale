using CustomerService.Core.Ports.In;
using CustomerService.Core.Ports.Out;

namespace CustomerService.Core.UseCases;

public class ExistsCustomerUseCase(ICustomerRepository repo) : IExistsCustomerUseCase
{
    public Task<bool> ExecuteAsync(string customerId, CancellationToken ct)
        => repo.ExistsAsync(customerId, ct);
}