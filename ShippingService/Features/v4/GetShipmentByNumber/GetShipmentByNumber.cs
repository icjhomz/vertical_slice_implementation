using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Abstract;
using ShippingService.Database;
using ShippingService.Extensions;
using ShippingService.Features.v4.CreateShipment;

namespace ShippingService.Features.v4.GetShipmentByNumber;

public class GetShipmentByNumberEndpoint : IEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapGet("/api/v4/shipments/{shipmentNumber}", Handle);
	}
	
	private static async Task<IResult> Handle(
		[FromRoute] string shipmentNumber,
		EfCoreDbContext context,
		ILogger<GetShipmentByNumberEndpoint> logger,
		CancellationToken cancellationToken)
	{
		var shipment = await context.Shipments
			.Include(x => x.Items)
			.Where(s => s.Number == shipmentNumber)
			.FirstOrDefaultAsync(cancellationToken);

		if (shipment is null)
		{
			logger.LogDebug("Shipment with number {ShipmentNumber} not found", shipmentNumber);
			return Error.NotFound($"Shipment with number '{shipmentNumber}' not found").ToProblem();
		}

		var response = shipment.MapToResponse();
		return Results.Ok(response);
	}
}
