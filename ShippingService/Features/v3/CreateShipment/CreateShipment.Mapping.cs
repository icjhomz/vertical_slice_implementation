using ShippingService.Database;
using ShippingService.Features.v3.Shared.Responses;
using ShippingService.SharedModels;

namespace ShippingService.Features.v3.CreateShipment;

internal static class CreateShipmentMappingExtensions
{
    public static CreateShipmentCommand MapToCommand(this CreateShipmentRequest request)
        => new(request.OrderId,
            request.Address,
            request.Carrier,
            request.ReceiverEmail,
            request.Items);
    
    public static Shipment MapToShipment(this CreateShipmentCommand command, string shipmentNumber)
        => new()
        {
            Number = shipmentNumber,
            OrderId = command.OrderId,
            Address = command.Address,
            Carrier = command.Carrier,
            ReceiverEmail = command.ReceiverEmail,
            Items = command.Items,
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

    public static ShipmentResponse MapToResponse(this Shipment shipment)
        => new(
            shipment.Number,
            shipment.OrderId,
            shipment.Address,
            shipment.Carrier,
            shipment.ReceiverEmail,
            shipment.Status,
            shipment.Items
                .Select(x => new ShipmentItemResponse(x.Product, x.Quantity))
                .ToList()
            );
}