using CustomerService.Core.Domain.Entities;
using CustomerService.Core.Ports.In;
using CustomerService.Core.Ports.Out;

namespace CustomerService.Core.UseCases;

public class GetCustomerUseCase(ICustomerRepository repo) : IGetCustomerUseCase
{
    public Task<Customer?> ExecuteAsync(string customerId, CancellationToken ct)
        => repo.GetByIdAsync(customerId, ct);
}