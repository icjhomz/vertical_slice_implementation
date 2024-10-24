using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Database;
using ShippingService.Extensions;
using ShippingService.SharedModels;

namespace ShippingService.Features.v2;

public static class UpdateShipmentStatus
{
	public sealed record Request(ShipmentStatus Status);

	internal sealed record Command(string ShipmentNumber, ShipmentStatus Status)
		: IRequest<ErrorOr<Success>>;

	internal sealed class CommandHandler(
		EfCoreDbContext context,
		ILogger<Command> logger)
		: IRequestHandler<Command, ErrorOr<Success>>
	{
		public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
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

	public class Validator : AbstractValidator<Request>
	{
		public Validator()
		{
			RuleFor(x => x.Status).IsInEnum();
		}
	}

	public static void MapEndpoint(WebApplication app)
	{
		app.MapPost("/api/v2/shipments/update-status/{shipmentNumber}",
			async (
				[FromRoute] string shipmentNumber,
				[FromBody] Request request,
				IValidator<Request> validator,
				IMediator mediator) =>
			{
				var validationResult = await validator.ValidateAsync(request);
				if (!validationResult.IsValid)
				{
					return Results.ValidationProblem(validationResult.ToDictionary());
				}

				var command = new Command(shipmentNumber, request.Status);

				var response = await mediator.Send(command);
				if (response.IsError)
				{
					return response.Errors.ToProblem();
				}

				return Results.NoContent();
			});
	}
}
