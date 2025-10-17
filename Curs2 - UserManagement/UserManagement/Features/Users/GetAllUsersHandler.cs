using Microsoft.EntityFrameworkCore;
using UserManagement.Persistence;

namespace UserManagement.Features.Users;

public class GetAllUsersHandler(UserManagementContext context)
{
    private readonly UserManagementContext _context = context;
    
    public async Task<IResult> Handle(GetAllUsersRequest request)
    {
        var users = await _context.Users.ToListAsync();
        return Results.Ok(users);
    }
}