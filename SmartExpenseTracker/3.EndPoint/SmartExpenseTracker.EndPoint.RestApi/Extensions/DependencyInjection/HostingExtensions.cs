using Microsoft.OpenApi.Models;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Extensions.DependencyInjection;
using SmartExpenseTracker.Extensions.DependencyInjection;
using SmartExpenseTracker.Infra.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using SmartExpenseTracker.EndPoint.RestApi.Middleware;
using SmartExpenseTracker.Infra.Persistence.Context;
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
                    Title = "Smart Expense Tracker API",
                    Version = "v1",
                    Description = "API برای مدیریت هزینه‌ها"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "توکن JWT را به این صورت وارد کنید: Bearer {your token}"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                    {
                      new OpenApiSecurityScheme
                         {
                           Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
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

            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();
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
            

            //Add Persistence
            builder.Services.RegisterPersistenceService(builder.Configuration);

            //Add Resilience
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
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseGlobalException(); // Global Exception Handleing
            app.UseResponseFramework(); // Optional middleware
            app.MapControllers();

            app.UseHttpsRedirection();

            app.MapControllers();

            app.UseMetericsMiddleware();
            return app;
        }
    }
}
