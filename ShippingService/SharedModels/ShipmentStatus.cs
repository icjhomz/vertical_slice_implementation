namespace ShippingService.SharedModels;

public enum ShipmentStatus
{
	Created,
	Processing,
	Dispatched,
	InTransit,
	WaitingCustomer,
	Delivered,
	Cancelled
}
