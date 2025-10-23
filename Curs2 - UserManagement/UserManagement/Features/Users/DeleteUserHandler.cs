using UserManagement.Persistence;

namespace UserManagement.Features.Users;

public class DeleteUserHandler(UserManagementContext context)
{
    public async Task<IResult> Handle(DeleteUserRequest request)
    {
        var user = await context.Users.FindAsync(request.Id);
        if (user == null)
        {
            return Results.NotFound();
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}