namespace ShippingService.SharedModels;

public class Address
{
	public required string Street { get; set; }

	public required string City { get; set; }

	public required string Zip { get; set; }
}