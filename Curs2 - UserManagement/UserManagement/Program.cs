using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Features.Users;
using UserManagement.Persistence;
using UserManagement.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<UserManagementContext>(options=>
    options.UseSqlite("Data Source=usermanagement.db"));
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<GetAllUsersHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/users", async ([FromBody]CreateUserRequest req,[FromServices] CreateUserHandler handler) =>
    await handler.Handle(req));
app.MapGet("/users", async ([FromServices] GetAllUsersHandler handler) =>
    await handler.Handle(new GetAllUsersRequest()));
app.MapDelete("/users/{id:guid}", async (Guid id, [FromServices] DeleteUserHandler handler) =>
    await handler.Handle(new DeleteUserRequest(id)));
app.Run();
