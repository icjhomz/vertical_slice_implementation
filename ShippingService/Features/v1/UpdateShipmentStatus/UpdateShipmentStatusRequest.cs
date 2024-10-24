using ShippingService.SharedModels;

namespace ShippingService.Features.v1.UpdateShipmentStatus;

public sealed record UpdateShipmentStatusRequest(ShipmentStatus Status);