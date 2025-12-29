namespace InventoryService.Core.Domain.Errors;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}