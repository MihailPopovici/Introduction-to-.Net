using BookManagement.Persistence;
using BookManagement.Validators;

namespace BookManagement.Features.Books;

public class UpdateBookHandler(BookManagementContext context)
{
    private readonly BookManagementContext _context = context;

    public async Task<IResult> Handle(UpdateBookRequest request)
    {
        var existing = await _context.Books.FindAsync(request.Id);
        if (existing == null)
            return Results.NotFound();

        var validator = new UpdateBookValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);
        
        var updated = new Book(existing.Id, request.Title, request.Author, request.Year);

        _context.Entry(existing).CurrentValues.SetValues(updated);
        await _context.SaveChangesAsync();

        return Results.Ok(updated);
    }
}