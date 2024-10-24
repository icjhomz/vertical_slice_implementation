using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingService.Database;

namespace ShippingService.Features.v1.GetShipmentByNumber;

internal sealed class GetShipmentByNumberQueryHandler(EfCoreDbContext context, ILogger<GetShipmentByNumberQueryHandler> logger)
    : IRequestHandler<GetShipmentByNumberQuery, ShipmentResponse?>
{
    public async Task<ShipmentResponse?> Handle(GetShipmentByNumberQuery request, CancellationToken cancellationToken)
    {
        var shipment = await context.Shipments
            .Include(x => x.Items)
            .Where(s => s.Number == request.ShipmentNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (shipment is null)
        {
            logger.LogDebug("Shipment with number {ShipmentNumber} not found", request.ShipmentNumber);
            return null;
        }

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
}