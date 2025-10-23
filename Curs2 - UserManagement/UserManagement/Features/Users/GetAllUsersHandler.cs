using Microsoft.EntityFrameworkCore;
using UserManagement.Persistence;

namespace UserManagement.Features.Users;

public class GetAllUsersHandler(UserManagementContext context)
{
    public async Task<IResult> Handle(GetAllUsersRequest request)
    {
        var users = await context.Users.ToListAsync();
        return Results.Ok(users);
    }
}