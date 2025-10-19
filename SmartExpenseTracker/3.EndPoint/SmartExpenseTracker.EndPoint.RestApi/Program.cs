using SmartExpenseTracker.EndPoint.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddResponseFramework();
builder.Services.AddEndpointsApiExplorer();



var app = builder.ConfigureService();



app.ConfigurePipeline();



app.Run();
