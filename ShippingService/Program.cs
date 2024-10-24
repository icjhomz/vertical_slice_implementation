using Microsoft.EntityFrameworkCore;
using ShippingService;
using ShippingService.Database;
using ShippingService.Features.v2;
using ShippingService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddHostLogging();
builder.Services.AddWebHostInfrastructure(builder.Configuration);
builder.Services.RegisterEndpointsFromAssemblyContaining<IApiMarker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<EfCoreDbContext>();
	await dbContext.Database.MigrateAsync();

	var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
	await seedService.SeedDataAsync();
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
