namespace CustomerService.Api.Contracts.Responses;

public record CustomerResponse(
    string CustomerId,
    string FullName,
    string DocumentCpf,
    string Email,
    string Phone,
    string AddressLine1,
    string City,
    string State,
    string PostalCode
);
