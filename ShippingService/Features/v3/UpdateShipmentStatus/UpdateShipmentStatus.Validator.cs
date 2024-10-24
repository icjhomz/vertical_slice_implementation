using FluentValidation;

namespace ShippingService.Features.v3.UpdateShipmentStatus;

public class UpdateShipmentStatusRequestValidator : AbstractValidator<UpdateShipmentStatusRequest>
{
	public UpdateShipmentStatusRequestValidator()
	{
		RuleFor(x => x.Status).IsInEnum();
	}
}
