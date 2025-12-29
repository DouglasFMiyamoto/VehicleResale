namespace CustomerService.Core.Ports.In;

public interface IExistsCustomerUseCase
{
    Task<bool> ExecuteAsync(string customerId, CancellationToken ct);
}