using ErrorOr;
using MediatR;
using ShippingService.SharedModels;

namespace ShippingService.Features.v1.UpdateShipmentStatus;

internal sealed record UpdateShipmentStatusCommand(string ShipmentNumber, ShipmentStatus Status)
    : IRequest<ErrorOr<Success>>;