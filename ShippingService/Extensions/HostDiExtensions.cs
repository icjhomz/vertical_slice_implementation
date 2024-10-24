using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using ShippingService;
using ShippingService.Database;
using ShippingService.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class HostDiExtensions
{
	public static IServiceCollection AddWebHostInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddScoped<SeedService>();

		services
			.AddEfCore(configuration);
		
		services
			.AddEndpointsApiExplorer()
			.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API v1", Version = "v1" });
				c.SwaggerDoc("v2", new OpenApiInfo { Title = "My API v2", Version = "v2" });
				c.SwaggerDoc("v3", new OpenApiInfo { Title = "My API v3", Version = "v3" });
				c.SwaggerDoc("v4", new OpenApiInfo { Title = "My API v4", Version = "v4" });
				
				c.CustomSchemaIds(type => type.FullName!.Replace("+", "."));
			});

		services.AddMediatR(options =>
		{
			options.RegisterServicesFromAssemblyContaining<EfCoreDbContext>();
		});

		services.Configure<JsonOptions>(opt =>
		{
			opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
		});

		services.AddValidatorsFromAssemblyContaining<IApiMarker>();

		return services;
	}

	public static void AddHostLogging(this WebApplicationBuilder builder)
	{
		builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));
	}

	private static IServiceCollection AddEfCore(this IServiceCollection services, IConfiguration configuration)
	{
		var postgresConnectionString = configuration.GetConnectionString("Postgres");

		services.AddDbContext<EfCoreDbContext>(x => x
			.EnableSensitiveDataLogging()
			.UseNpgsql(postgresConnectionString, npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__MyMigrationsHistory", "shipping"))
			.UseSnakeCaseNamingConvention()
		);

		return services;
	}
}
