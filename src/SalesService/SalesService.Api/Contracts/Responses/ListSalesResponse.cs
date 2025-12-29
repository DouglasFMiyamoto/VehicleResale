using SalesService.Core.Ports.In;

namespace SalesService.Api.Contracts.Responses;

public sealed record ListSalesResponse(List<SaleListItem> Items);