using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingService.Database;

namespace ShippingService.Features.v1.UpdateShipmentStatus;

internal sealed class UpdateShipmentStatusCommandHandler(
    EfCoreDbContext context,
    ILogger<UpdateShipmentStatusCommand> logger)
    : IRequestHandler<UpdateShipmentStatusCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var shipment = await context.Shipments
            .Where(x => x.Number == request.ShipmentNumber)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (shipment is null)
        {
            logger.LogDebug("Shipment with number {ShipmentNumber} not found", request.ShipmentNumber);
            return Error.NotFound("Shipment.NotFound", $"Shipment with number '{request.ShipmentNumber}' not found");
        }

        shipment.Status = request.Status;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated state of shipment {ShipmentNumber} to {NewState}", request.ShipmentNumber, request.Status);

        return Result.Success;
    }
}