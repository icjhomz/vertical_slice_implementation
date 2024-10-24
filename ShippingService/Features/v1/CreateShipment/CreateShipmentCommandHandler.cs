using Bogus;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingService.Database;
using ShippingService.SharedModels;

namespace ShippingService.Features.v1.CreateShipment;

internal sealed class CreateShipmentCommandHandler(
    EfCoreDbContext context,
    ILogger<CreateShipmentCommandHandler> logger)
    : IRequestHandler<CreateShipmentCommand, ErrorOr<ShipmentResponse>>
{
    public async Task<ErrorOr<ShipmentResponse>> Handle(
        CreateShipmentCommand request,
        CancellationToken cancellationToken)
    {
        var shipmentAlreadyExists = await context.Shipments
            .Where(s => s.OrderId == request.OrderId)
            .AnyAsync(cancellationToken);

        if (shipmentAlreadyExists)
        {
            logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
            return Error.Conflict($"Shipment for order '{request.OrderId}' is already created");
        }

        var shipmentNumber = new Faker().Commerce.Ean8();
        var shipment = CreateShipment(request, shipmentNumber);

        context.Shipments.Add(shipment);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created shipment: {@Shipment}", shipment);

        return new ShipmentResponse(
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

    private static Shipment CreateShipment(CreateShipmentCommand request, string shipmentNumber)
    {
        return new Shipment
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
    }
}