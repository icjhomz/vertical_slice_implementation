using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShippingService.Abstract;
using ShippingService.Extensions;

namespace ShippingService.Features.v1.UpdateShipmentStatus;

public class UpdateShipmentStatusEndpoint : IEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPost("/api/v1/shipments/update-status/{shipmentNumber}", Handle);
	}
	
	private static async Task<IResult> Handle(
		[FromRoute] string shipmentNumber,
		[FromBody] UpdateShipmentStatusRequest request,
		IValidator<UpdateShipmentStatusRequest> validator,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var validationResult = await validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
		{
			return Results.ValidationProblem(validationResult.ToDictionary());
		}

		var command = new UpdateShipmentStatusCommand(shipmentNumber, request.Status);

		var response = await mediator.Send(command, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.NoContent();
	}
}
