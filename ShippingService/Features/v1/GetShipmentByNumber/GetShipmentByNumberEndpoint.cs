using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShippingService.Abstract;

namespace ShippingService.Features.v1.GetShipmentByNumber;

public class GetShipmentByNumberEndpoint : IEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapGet("/api/v1/shipments/{shipmentNumber}", Handle);
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
