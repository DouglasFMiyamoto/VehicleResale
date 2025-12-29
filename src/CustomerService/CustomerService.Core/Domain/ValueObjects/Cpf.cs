using CustomerService.Core.Domain.Errors;

namespace CustomerService.Core.Domain.ValueObjects;

public sealed class Cpf
{
    public string Value { get; }

    public Cpf(string value)
    {
        value = (value ?? "").Trim();
        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 11)
            throw new DomainException("CPF inválido (deve conter 11 dígitos).");

        if (digits.Distinct().Count() == 1)
            throw new DomainException("CPF inválido.");

        Value = digits;
    }

    public override string ToString() => Value;
}