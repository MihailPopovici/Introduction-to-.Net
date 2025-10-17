using BookManagement.Persistence;

namespace BookManagement.Features.Books;

public class DeleteBookHandler(BookManagementContext context)
{
    private readonly BookManagementContext _context = context;

    public async Task<IResult> Handle(DeleteBookRequest request)
    {
        var book = await _context.Books.FindAsync(request.Id);
        if (book == null)
            return Results.NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return Results.NoContent();
    }
}