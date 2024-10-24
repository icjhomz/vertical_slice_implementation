using MediatR;

namespace ShippingService.Features.v1.GetShipmentByNumber;

internal sealed record GetShipmentByNumberQuery(string ShipmentNumber)
    : IRequest<ShipmentResponse?>;