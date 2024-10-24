using ShippingService.Database;
using ShippingService.Features.v4.Shared.Responses;
using ShippingService.SharedModels;

namespace ShippingService.Features.v4.CreateShipment;

public static class CreateShipmentMappingExtensions
{
    public static Shipment MapToShipment(this CreateShipmentRequest request, string shipmentNumber)
        => new()
        {
            Number = shipmentNumber,
            OrderId = request.OrderId,
            Address = request.Address,
            Carrier = request.Carrier,
            ReceiverEmail = request.ReceiverEmail,
            Items = request.Items,
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