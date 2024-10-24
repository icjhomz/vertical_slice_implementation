using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Abstract;
using ShippingService.Database;
using ShippingService.Extensions;
using ShippingService.SharedModels;

namespace ShippingService.Features.v3.UpdateShipmentStatus;

public sealed record UpdateShipmentStatusRequest(ShipmentStatus Status);

internal sealed record UpdateShipmentStatusCommand(string ShipmentNumber, ShipmentStatus Status)
	: IRequest<ErrorOr<Success>>;

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

public class UpdateShipmentStatusEndpoint : IEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPost("/api/v3/shipments/update-status/{shipmentNumber}", Handle);
	}

	private static async Task<IResult> Handle(
		[FromRoute] string shipmentNumber,
		[FromBody] UpdateShipmentStatusRequest request,
		IValidator<UpdateShipmentStatusRequest> validator,
		IMediator mediator)
	{
		var validationResult = await validator.ValidateAsync(request);
		if (!validationResult.IsValid)
		{
			return Results.ValidationProblem(validationResult.ToDictionary());
		}

		var command = new UpdateShipmentStatusCommand(shipmentNumber, request.Status);

		var response = await mediator.Send(command);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.NoContent();
	}
}
