using Bogus;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Abstract;
using ShippingService.Database;
using ShippingService.Extensions;
using ShippingService.Features.v3.Shared.Responses;
using ShippingService.SharedModels;

namespace ShippingService.Features.v3.CreateShipment;

public sealed record CreateShipmentRequest(
    string OrderId,
    Address Address,
    string Carrier,
    string ReceiverEmail,
    List<ShipmentItem> Items);

internal sealed record CreateShipmentCommand(
    string OrderId,
    Address Address,
    string Carrier,
    string ReceiverEmail,
    List<ShipmentItem> Items)
    : IRequest<ErrorOr<ShipmentResponse>>;

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
        var shipment = request.MapToShipment(shipmentNumber);

        context.Shipments.Add(shipment);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created shipment: {@Shipment}", shipment);

        var response = shipment.MapToResponse();
        return response;
    }
}

public class CreateShipmentEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/v3/shipments", Handle);
    }
    
    private static async Task<IResult> Handle(
        [FromBody] CreateShipmentRequest request,
        IValidator<CreateShipmentRequest> validator,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
                
        var command = request.MapToCommand();

        var response = await mediator.Send(command, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}