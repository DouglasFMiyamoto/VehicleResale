using CustomerService.Core.Domain.Entities;

namespace CustomerService.Core.Ports.Out;

public interface ICustomerRepository
{
    Task CreateAsync(Customer customer, CancellationToken ct);
    Task<Customer?> GetByIdAsync(string customerId, CancellationToken ct);
    Task<bool> ExistsAsync(string customerId, CancellationToken ct);
}