using InventoryService.Core.Ports.In;

namespace InventoryService.Api.Contracts.Responses;

public sealed record ListVehiclesResponse(List<VehicleListItem> Items);