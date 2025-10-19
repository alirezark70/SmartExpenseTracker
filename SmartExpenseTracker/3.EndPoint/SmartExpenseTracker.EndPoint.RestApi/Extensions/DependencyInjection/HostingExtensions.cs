using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Extensions.DependencyInjection;
using SmartExpenseTracker.EndPoint.RestApi.Middleware;
using SmartExpenseTracker.Extensions.DependencyInjection;
using SmartExpenseTracker.Infra.Extensions.DependencyInjection;
using SmartExpenseTracker.Infra.Persistence.Configuration;
using SmartExpenseTracker.Infra.Persistence.Context;
using Swashbuckle.AspNetCore.Filters;
using System.Configuration;
using System.Text;
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

            builder.Logging.ClearProviders();

            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .WriteTo.Seq(
                        serverUrl: "http://localhost:5341",
                        restrictedToMinimumLevel: LogEventLevel.Verbose)
                    .ReadFrom.Configuration(context.Configuration);
            });

            builder.Services.AddOpenApi();


            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
                options.AddPolicy("AdminOrUser", policy => policy.RequireRole("Admin", "User"));
            });


            // JWT Authentication
            var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // در Production باید true باشد
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings!.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero,
                    LifetimeValidator = (notBefore, expires, token, parameters) =>
                    {
                        return expires != null && expires > DateTime.UtcNow;
                    }
                };

                // Event handlers
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        // برای SignalR
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

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

            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
                };
            });
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

            Log.Information("Application Starting...");
            return app;
        }
    }
}
