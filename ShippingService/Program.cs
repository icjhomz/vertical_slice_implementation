using MediatR;
using ShippingService;
using ShippingService.Features.v2;

var builder = WebApplication.CreateBuilder(args);
builder.AddHostLogging();
builder.Services.AddWebHostInfrastructure(builder.Configuration);
builder.Services.RegisterEndpointsFromAssemblyContaining<IApiMarker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

	var shipmentNumberHardCoded = "4380245613123";

    var response = await mediator.Send(new GetShipmentByNumber.Query(shipmentNumberHardCoded));

    if (response != null)
    {
        Console.WriteLine($"Shipment found at startup: {response.Number}");
    }
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapEndpoints();

CreateShipment.MapEndpoint(app);
UpdateShipmentStatus.MapEndpoint(app);
GetShipmentByNumber.MapEndpoint(app);

app.Run();
