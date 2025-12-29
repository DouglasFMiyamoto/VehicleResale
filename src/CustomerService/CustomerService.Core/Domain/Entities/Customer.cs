using CustomerService.Core.Domain.ValueObjects;
using CustomerService.Core.Domain.Errors;

namespace CustomerService.Core.Domain.Entities;

public class Customer
{
    public string CustomerId { get; private set; } = Guid.NewGuid().ToString("N");
    public string FullName { get; private set; } = "";
    public Cpf Document { get; private set; } = new Cpf("12345678901"); 
    public Email Email { get; private set; } = new Email("temp@temp.com");
    public string Phone { get; private set; } = "";
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public string AddressLine1 { get; private set; } = "";
    public string City { get; private set; } = "";
    public string State { get; private set; } = "";
    public string PostalCode { get; private set; } = "";

    private Customer() { }

    public Customer(
        string fullName,
        string cpf,
        string email,
        string phone,
        string addressLine1,
        string city,
        string state,
        string postalCode)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Nome é obrigatório.");

        FullName = fullName.Trim();
        Document = new Cpf(cpf);
        Email = new Email(email);
        Phone = (phone ?? "").Trim();

        AddressLine1 = (addressLine1 ?? "").Trim();
        City = (city ?? "").Trim();
        State = (state ?? "").Trim();
        PostalCode = new string((postalCode ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        if (AddressLine1.Length < 5) throw new DomainException("Endereço inválido.");
        if (City.Length < 2) throw new DomainException("Cidade inválida.");
        if (State.Length < 2) throw new DomainException("Estado inválido.");
        if (PostalCode.Length < 5) throw new DomainException("CEP inválido.");

        CustomerId = Guid.NewGuid().ToString("N");
        CreatedAtUtc = DateTime.UtcNow;
    }
}