using CustomerService.Core.Domain.Entities;

namespace CustomerService.Core.Ports.In;

public interface IGetCustomerUseCase
{
    Task<Customer?> ExecuteAsync(string customerId, CancellationToken ct);
}