using CustomerService.Core.Domain.Entities;
using CustomerService.Core.Domain.ValueObjects;

namespace CustomerService.Core.Domain.Entities;

public static class CustomerFactory
{
    public static Customer Rehydrate(
        string customerId,
        string fullName,
        string cpfDigits,
        string email,
        string phone,
        string addressLine1,
        string city,
        string state,
        string postalCode,
        DateTime createdAtUtc)
    {
        var c = (Customer)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Customer));

        typeof(Customer).GetProperty(nameof(Customer.CustomerId))!.SetValue(c, customerId);
        typeof(Customer).GetProperty(nameof(Customer.FullName))!.SetValue(c, fullName);
        typeof(Customer).GetProperty(nameof(Customer.Document))!.SetValue(c, new Cpf(cpfDigits));
        typeof(Customer).GetProperty(nameof(Customer.Email))!.SetValue(c, new Email(email));
        typeof(Customer).GetProperty(nameof(Customer.Phone))!.SetValue(c, phone);
        typeof(Customer).GetProperty(nameof(Customer.AddressLine1))!.SetValue(c, addressLine1);
        typeof(Customer).GetProperty(nameof(Customer.City))!.SetValue(c, city);
        typeof(Customer).GetProperty(nameof(Customer.State))!.SetValue(c, state);
        typeof(Customer).GetProperty(nameof(Customer.PostalCode))!.SetValue(c, postalCode);
        typeof(Customer).GetProperty(nameof(Customer.CreatedAtUtc))!.SetValue(c, createdAtUtc);

        return c;
    }
}
