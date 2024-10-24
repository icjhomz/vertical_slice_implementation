using Bogus;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Abstract;
using ShippingService.Database;
using ShippingService.Extensions;
using ShippingService.SharedModels;

namespace ShippingService.Features.v4.CreateShipment;

public sealed record CreateShipmentRequest(
    string OrderId,
    Address Address,
    string Carrier,
    string ReceiverEmail,
    List<ShipmentItem> Items);

public class CreateShipmentEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/v4/shipments", Handle);
    }
    
    private static async Task<IResult> Handle(
        [FromBody] CreateShipmentRequest request,
        IValidator<CreateShipmentRequest> validator,
        EfCoreDbContext context,
        ILogger<CreateShipmentEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
                
        var shipmentAlreadyExists = await context.Shipments
            .Where(s => s.OrderId == request.OrderId)
            .AnyAsync(cancellationToken);

        if (shipmentAlreadyExists)
        {
            logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
            return Error.Conflict($"Shipment for order '{request.OrderId}' is already created").ToProblem();
        }

        var shipment = await CreateShipmentAsync(request, context, cancellationToken);

        logger.LogInformation("Created shipment: {@Shipment}", shipment);

        var response = shipment.MapToResponse();
        return Results.Ok(response);
    }

    private static async Task<Shipment> CreateShipmentAsync(CreateShipmentRequest request, EfCoreDbContext context, CancellationToken cancellationToken)
    {
        var shipmentNumber = new Faker().Commerce.Ean8();
        var shipment = request.MapToShipment(shipmentNumber);

        context.Shipments.Add(shipment);
        await context.SaveChangesAsync(cancellationToken);
        return shipment;
    }
}