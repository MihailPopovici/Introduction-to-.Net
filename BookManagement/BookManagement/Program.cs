using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookManagement.Features.Books;
using BookManagement.Persistence;
using BookManagement.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<BookManagementContext>(options =>
    options.UseSqlite("Data Source=bookmanagement.db"));

builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<GetAllBooksHandler>();
builder.Services.AddScoped<GetBookByIdHandler>();
builder.Services.AddScoped<UpdateBookHandler>();
builder.Services.AddScoped<DeleteBookHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateBookValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BookManagementContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoints
app.MapPost("/books", async ([FromBody] CreateBookRequest req, [FromServices] CreateBookHandler handler) =>
    await handler.Handle(req));

app.MapGet("/books", async (int? page, int? pageSize, [FromServices] GetAllBooksHandler handler) =>
{
    var req = new GetAllBooksRequest(page ?? 1, pageSize ?? 10);
    return await handler.Handle(req);
});

app.MapGet("/books/{id:int}", async (int id, [FromServices] GetBookByIdHandler handler) =>
await handler.Handle(new GetBookByIdRequest(id)));

app.MapPut("/books/{id:int}", async (int id, [FromBody] UpdateBookRequest bodyReq, [FromServices] UpdateBookHandler handler) =>
{
    var req = bodyReq with { Id = id };
    return await handler.Handle(req);
});

app.MapDelete("/books/{id:int}", async (int id, [FromServices] DeleteBookHandler handler) =>
await handler.Handle(new DeleteBookRequest(id)));

app.Run();

