namespace CustomerService.Core.Ports.Out;

public interface ICustomerDataProtector
{
    string ProtectSensitive(string plaintext);
    string UnprotectSensitive(string protectedValue);
}
