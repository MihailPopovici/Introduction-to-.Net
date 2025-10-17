using Microsoft.EntityFrameworkCore;
using BookManagement.Persistence;

namespace BookManagement.Features.Books;

public class GetAllBooksHandler(BookManagementContext context)
{
    private readonly BookManagementContext _context = context;

    public async Task<IResult> Handle(GetAllBooksRequest request)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Books.AsQueryable();
        
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(b => b.Id) 
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var result = new
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return Results.Ok(result);
    }
}