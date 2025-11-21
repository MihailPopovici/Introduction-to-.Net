using FluentValidation;
using MediatR;
using Microsoft.OpenApi.Models;
using OrderManagement.Common.Mapping;
using OrderManagement.Common.Middleware;
using OrderManagement.Features.Orders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

// Register AutoMapper profiles
builder.Services.AddAutoMapper(typeof(OrderMappingProfile), typeof(AdvancedOrderMappingProfile));

// Register validators
builder.Services.AddScoped<CreateOrderProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderProfileValidator>();

builder.Services.AddMediatR(typeof(CreateOrderHandler).Assembly);

// OpenAPI / Swagger (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrderManagement API",
        Version = "v1",
        Description = "API for managing orders"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<CorrelationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderManagement API v1"));
}

app.UseHttpsRedirection();

// Orders endpoint - create order
app.MapPost("/orders", async (IMediator mediator, CreateOrderProfileRequest request, CancellationToken ct) =>
{
    var result = await mediator.Send(request, ct);
    return Results.Created($"/orders/{result.Id}", result);
})
.WithName("CreateOrder")
.WithTags("Orders")
.WithOpenApi();

app.Run();
