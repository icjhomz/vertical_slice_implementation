using ErrorOr;
using MediatR;
using ShippingService.Database;
using ShippingService.SharedModels;

namespace ShippingService.Features.v1.CreateShipment;

internal sealed record CreateShipmentCommand(
    string OrderId,
    Address Address,
    string Carrier,
    string ReceiverEmail,
    List<ShipmentItem> Items)
    : IRequest<ErrorOr<ShipmentResponse>>;