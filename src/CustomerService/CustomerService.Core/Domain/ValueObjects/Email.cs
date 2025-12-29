using System.Net.Mail;
using CustomerService.Core.Domain.Errors;

namespace CustomerService.Core.Domain.ValueObjects;

public sealed class Email
{
     public string Value { get; }

     public Email(string value)
     {
         value = (value ?? "").Trim();

         try
         {
             var _ = new MailAddress(value);
             Value = value.ToLowerInvariant();
         }
         catch
         {
             throw new DomainException("E-mail inválido.");
         }
    }

     public override string ToString() => Value;
}