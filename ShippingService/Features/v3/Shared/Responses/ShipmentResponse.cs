using ShippingService.SharedModels;

namespace ShippingService.Features.v3.Shared.Responses;

public sealed record ShipmentResponse(
    string Number,
    string OrderId,
    Address Address,
    string Carrier,
    string ReceiverEmail,
    ShipmentStatus Status,
    List<ShipmentItemResponse> Items);