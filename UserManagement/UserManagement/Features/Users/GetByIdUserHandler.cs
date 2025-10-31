using UserManagement.Exceptions;
using UserManagement.Persistence;

namespace UserManagement.Features.Users;

public class GetByIdUserHandler(UserManagementContext dbContext)
{
    public async Task<IResult> Handle(GetByIdUserRequest request)
    {
        var user = await dbContext.Users.FindAsync(request.Id);
        if (user == null)
        {
            throw new UserNotFoundException(request.Id);
        }

        return Results.Ok(user);
    }
}