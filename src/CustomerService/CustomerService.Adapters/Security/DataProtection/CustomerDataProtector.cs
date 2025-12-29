using System.Security.Cryptography;
using CustomerService.Core.Ports.Out;
using Microsoft.AspNetCore.DataProtection;

namespace CustomerService.Adapters.Security.DataProtection;

public class CustomerDataProtector : ICustomerDataProtector
{
    private readonly IDataProtector _protector;

    public CustomerDataProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("CustomerService.SensitiveData.v1");
    }

    public string ProtectSensitive(string plaintext)
        => string.IsNullOrWhiteSpace(plaintext) ? plaintext : _protector.Protect(plaintext);

    public string UnprotectSensitive(string protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
            return protectedValue;

        if (!protectedValue.StartsWith("CfDJ8", StringComparison.Ordinal))
            return protectedValue;

        try
        {
            return _protector.Unprotect(protectedValue);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Unprotect failed: {ex.GetType().Name} - {ex.Message}");
            return protectedValue;
        }
    }

}
