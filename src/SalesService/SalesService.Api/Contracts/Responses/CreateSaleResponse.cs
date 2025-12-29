namespace SalesService.Api.Contracts.Responses;

public sealed record CreateSaleResponse(string SaleId, bool AlreadyExisted);