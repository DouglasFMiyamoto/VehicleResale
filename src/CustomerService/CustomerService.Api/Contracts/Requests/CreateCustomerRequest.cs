namespace CustomerService.Api.Contracts.Requests;

public class CreateCustomerRequest
{
    public string FullName { get; set; } = "";
    public string DocumentCpf { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string AddressLine1 { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";
}
