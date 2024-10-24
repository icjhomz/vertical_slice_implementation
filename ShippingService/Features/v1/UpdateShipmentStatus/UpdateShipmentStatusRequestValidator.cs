using FluentValidation;

namespace ShippingService.Features.v1.UpdateShipmentStatus;

public class UpdateShipmentStatusRequestValidator : AbstractValidator<UpdateShipmentStatus.UpdateShipmentStatusRequest>
{
	public UpdateShipmentStatusRequestValidator()
	{
		RuleFor(x => x.Status).IsInEnum();
	}
}
