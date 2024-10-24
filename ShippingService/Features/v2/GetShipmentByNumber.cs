using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Database;
using ShippingService.SharedModels;

namespace ShippingService.Features.v2;

internal static class GetShipmentByNumber
{
	internal sealed record Query(string ShipmentNumber)
		: IRequest<Response?>;

	internal sealed record Response(
		string Number,
		string OrderId,
		Address Address,
		string Carrier,
		string ReceiverEmail,
		ShipmentStatus Status,
		List<ShipmentItemResponse> Items);

	internal sealed record ShipmentItemResponse(string Product, int Quantity);

	internal sealed class QueryHandler(EfCoreDbContext context, ILogger<QueryHandler> logger)
		: IRequestHandler<Query, Response?>
	{
		public async Task<Response?> Handle(Query request, CancellationToken cancellationToken)
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

			return new Response(
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

	internal static void MapEndpoint(WebApplication app)
	{
		app.MapGet("/api/v2/shipments/{shipmentNumber}", async ([FromRoute] string shipmentNumber, IMediator mediator) =>
		{
			var response = await mediator.Send(new Query(shipmentNumber));
			return response is not null ? Results.Ok(response) : Results.NotFound($"Shipment with number '{shipmentNumber}' not found");
		});
	}
}
