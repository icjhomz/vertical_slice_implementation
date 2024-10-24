using ShippingService.Database;
using ShippingService.SharedModels;

namespace ShippingService.Features.v1.CreateShipment;

public sealed record CreateShipmentRequest(
    string OrderId,
    Address Address,
    string Carrier,
    string ReceiverEmail,
    List<ShipmentItem> Items);