using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShippingService.Abstract;
using ShippingService.Extensions;

namespace ShippingService.Features.v1.CreateShipment;

public class CreateShipmentEndpoint : IEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPost("/api/v1/shipments", Handle);
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

		var command = new CreateShipmentCommand(
			request.OrderId,
			request.Address,
			request.Carrier,
			request.ReceiverEmail,
			request.Items);

		var response = await mediator.Send(command, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.Ok(response.Value);
	}
}
