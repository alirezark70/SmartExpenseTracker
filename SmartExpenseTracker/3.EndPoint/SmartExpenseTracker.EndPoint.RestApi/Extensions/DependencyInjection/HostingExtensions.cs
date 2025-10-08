using Microsoft.OpenApi.Models;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using Swashbuckle.AspNetCore.Filters;
using SmartExpenseTracker.Infra.Extensions.DependencyInjection;
using SmartExpenseTracker.Extensions.DependencyInjection;
using SmartExpenseTracker.Core.Extensions.DependencyInjection;

namespace SmartExpenseTracker.EndPoint.Extensions.DependencyInjection
{
    public static class HostingExtensions
    {
        public static WebApplication ConfigureService(this WebApplicationBuilder builder)
        {
            //var connectionString =
            //    builder.Configuration.GetConnectionString("CnnString") ??
            //    "throw new ArgumentNullException(nameof(configuration))";

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1"
                });

                // Add response headers
                c.OperationFilter<AddResponseHeadersFilter>();

                // Define response types
                c.MapType<ApiResponse<object>>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["isSuccess"] = new OpenApiSchema { Type = "boolean" },
                        ["statusCode"] = new OpenApiSchema { Type = "integer" },
                        ["message"] = new OpenApiSchema { Type = "string" },
                        ["data"] = new OpenApiSchema { Type = "object" },
                        ["timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time" },
                        ["traceId"] = new OpenApiSchema { Type = "string" }
                    }
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });


            builder.Services.AddOpenApi();
            //builder.Services.AddDbContext<HouseRentDbContext>(options =>
            //{
            //    options.UseSqlServer(connectionString);
            //});

            builder.Services.RegisterResilienceService(builder.Configuration);

            //Add Id Generator Services
            builder.Services.RegisterSnowflakeIdGeneratorService(1);

            //Add internal Services
            builder.Services.RegisterSimpleDateTimeService();

            //Add Mapping Services
            builder.Services.RegisterMappingService();

            // add external service and resilince and telemetry and retry services
            builder.Services.RegisterExternalService(builder.Configuration);

            //Add application Services
            builder.Services.RegisterApplicationService();


            builder.Services.AddMetericsToDI();

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseFramework(); // Optional middleware
            app.MapControllers();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseMetericsMiddleware();
            return app;
        }
    }
}
