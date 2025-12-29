using CustomerService.Core.Domain.Entities;

namespace CustomerService.Core.Ports.In;

public interface ICreateCustomerUseCase
{
    Task<Customer> ExecuteAsync(CreateCustomerCommand cmd, CancellationToken ct);
}

public record CreateCustomerCommand(
    string FullName,
    string DocumentCpf,
    string Email,
    string Phone,
    string AddressLine1,
    string City,
    string State,
    string PostalCode
);