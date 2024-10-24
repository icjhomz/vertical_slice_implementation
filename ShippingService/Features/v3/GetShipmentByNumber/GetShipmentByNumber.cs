using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Abstract;
using ShippingService.Database;
using ShippingService.Features.v3.CreateShipment;
using ShippingService.Features.v3.Shared.Responses;

namespace ShippingService.Features.v3.GetShipmentByNumber;

internal sealed record GetShipmentByNumberQuery(string ShipmentNumber)
	: IRequest<ShipmentResponse?>;

internal sealed class GetShipmentByNumberQueryHandler(
	EfCoreDbContext context,
	ILogger<GetShipmentByNumberQueryHandler> logger)
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

		var response = shipment.MapToResponse();
		return response;
	}
}

public class GetShipmentByNumberEndpoint : IEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapGet("/api/v3/shipments/{shipmentNumber}", Handle);
	}
	
	private static async Task<IResult> Handle(
		[FromRoute] string shipmentNumber,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var response = await mediator.Send(new GetShipmentByNumberQuery(shipmentNumber), cancellationToken);
		return response is not null ? Results.Ok(response) : Results.NotFound($"Shipment with number '{shipmentNumber}' not found");
	}
}
