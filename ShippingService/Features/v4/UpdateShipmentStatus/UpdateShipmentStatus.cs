using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Abstract;
using ShippingService.Database;
using ShippingService.Extensions;
using ShippingService.SharedModels;

namespace ShippingService.Features.v4.UpdateShipmentStatus;

public sealed record UpdateShipmentStatusRequest(ShipmentStatus Status);

public class UpdateShipmentStatusEndpoint : IEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPost("/api/v4/shipments/update-status/{shipmentNumber}", Handle);
	}
	
	private static async Task<IResult> Handle(
		[FromRoute] string shipmentNumber,
		[FromBody] UpdateShipmentStatusRequest request,
		IValidator<UpdateShipmentStatusRequest> validator,
		EfCoreDbContext context,
		ILogger<UpdateShipmentStatusEndpoint> logger,
		CancellationToken cancellationToken)
	{
		var validationResult = await validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
		{
			return Results.ValidationProblem(validationResult.ToDictionary());
		}

		var shipment = await context.Shipments
			.Where(x => x.Number == shipmentNumber)
			.FirstOrDefaultAsync(cancellationToken: cancellationToken);

		if (shipment is null)
		{
			logger.LogDebug("Shipment with number {ShipmentNumber} not found", shipmentNumber);
			return Error.NotFound("Shipment.NotFound", $"Shipment with number '{shipmentNumber}' not found").ToProblem();
		}

		shipment.Status = request.Status;

		await context.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Updated state of shipment {ShipmentNumber} to {NewState}", shipmentNumber, request.Status);

		return Results.NoContent();
	}
}
