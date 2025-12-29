using CustomerService.Core.Domain.Entities;
using CustomerService.Core.Ports.In;
using CustomerService.Core.Ports.Out;

namespace CustomerService.Core.UseCases;

public class CreateCustomerUseCase(ICustomerRepository repo) : ICreateCustomerUseCase
{
    public async Task<Customer> ExecuteAsync(CreateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = new Customer(
             cmd.FullName, cmd.DocumentCpf, cmd.Email, cmd.Phone,
             cmd.AddressLine1, cmd.City, cmd.State, cmd.PostalCode);

        await repo.CreateAsync(customer, ct);
        return customer;
    }
}
