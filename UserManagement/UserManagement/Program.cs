using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserManagement.Features.Users;
using UserManagement.Mappings;
using UserManagement.Middleware;
using UserManagement.Persistence;
using UserManagement.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<UserManagementContext>(options=>
    options.UseSqlite("Data Source=usermanagement.db"));

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<UserMappingProfile>(), typeof(UserMappingProfile));
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<GetAllUsersHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<GetByIdUserHandler>();
builder.Services.AddScoped<UpdateUserHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "UserManagement.Features.Users",
            Version = "v1",
            Description = "API for managing users",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "API Support",
                Email = "support@example.com"
            }
        }); 
});

var app = builder.Build();

//Ensure database is created at runtime
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserManagementContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API V1");
            c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
            c.DisplayRequestDuration();
        }); 
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseGlobalExceptionMiddleware();

app.MapPost("/users", async ([FromBody]CreateUserRequest req,[FromServices] CreateUserHandler handler) =>
    await handler.Handle(req));
app.MapGet("/users", async ([FromServices] GetAllUsersHandler handler) =>
    await handler.Handle(new GetAllUsersRequest()));
app.MapDelete("/users/{id:guid}", async (Guid id, [FromServices] DeleteUserHandler handler) =>
    await handler.Handle(new DeleteUserRequest(id)));
app.MapGet("/users/{id:guid}", async (Guid id, [FromServices] GetByIdUserHandler handler) =>
    await handler.Handle(new GetByIdUserRequest(id)));
//TODO - discuss this later
app.MapPut("/users/{id:guid}", async (Guid id, [FromBody] UpdateUserRequest req, [FromServices] UpdateUserHandler handler) =>
{
    await handler.Handle(req);
});
app.Run();
