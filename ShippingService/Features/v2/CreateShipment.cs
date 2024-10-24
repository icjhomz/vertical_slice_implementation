using Bogus;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Database;
using ShippingService.Extensions;
using ShippingService.SharedModels;

namespace ShippingService.Features.v2;

public static class CreateShipment
{
	public sealed record Request(
		string OrderId,
		Address Address,
		string Carrier,
		string ReceiverEmail,
		List<ShipmentItem> Items);

	public sealed record Response(
		string Number,
		string OrderId,
		Address Address,
		string Carrier,
		string ReceiverEmail,
		ShipmentStatus Status,
		List<ShipmentItemResponse> Items);

	public sealed record ShipmentItemResponse(string Product, int Quantity);

	internal sealed record Command(
		string OrderId,
		Address Address,
		string Carrier,
		string ReceiverEmail,
		List<ShipmentItem> Items)
		: IRequest<ErrorOr<Response>>;

	internal sealed class CommandHandler(
		EfCoreDbContext context,
		ILogger<CommandHandler> logger)
		: IRequestHandler<Command, ErrorOr<Response>>
	{
		public async Task<ErrorOr<Response>> Handle(
			Command request,
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
			var shipment = CreateShipment(request, shipmentNumber);

			context.Shipments.Add(shipment);
			await context.SaveChangesAsync(cancellationToken);

			logger.LogInformation("Created shipment: {@Shipment}", shipment);

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

		private static Shipment CreateShipment(Command request, string shipmentNumber)
		{
			return new Shipment
			{
				Number = shipmentNumber,
				OrderId = request.OrderId,
				Address = request.Address,
				Carrier = request.Carrier,
				ReceiverEmail = request.ReceiverEmail,
				Items = request.Items,
				Status = ShipmentStatus.Created,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = null
			};
		}
	}
	
	public class Validator : AbstractValidator<Request>
	{
		public Validator()
		{
			RuleFor(shipment => shipment.OrderId).NotEmpty();
			RuleFor(shipment => shipment.Carrier).NotEmpty();
			RuleFor(shipment => shipment.ReceiverEmail).NotEmpty();
			RuleFor(shipment => shipment.Items).NotEmpty();
			
			RuleFor(shipment => shipment.Address)
				.Cascade(CascadeMode.Stop)
				.NotNull()
				.WithMessage("Address must not be null")
				.SetValidator(new AddressValidator());
		}
	}

	public class AddressValidator : AbstractValidator<Address>
	{
		public AddressValidator()
		{
			RuleFor(address => address.Street).NotEmpty();
			RuleFor(address => address.City).NotEmpty();
			RuleFor(address => address.Zip).NotEmpty();
		}
	}

	public static void MapEndpoint(WebApplication app)
	{
		app.MapPost("/api/v2/shipments",
			async (
				[FromBody] Request request,
				IValidator<Request> validator,
				IMediator mediator) =>
			{
				var validationResult = await validator.ValidateAsync(request);
				if (!validationResult.IsValid)
				{
					return Results.ValidationProblem(validationResult.ToDictionary());
				}

				var command = new Command(
					request.OrderId,
					request.Address,
					request.Carrier,
					request.ReceiverEmail,
					request.Items);

				var response = await mediator.Send(command);
				if (response.IsError)
				{
					return response.Errors.ToProblem();
				}

				return Results.Ok(response.Value);
			});
	}
}
