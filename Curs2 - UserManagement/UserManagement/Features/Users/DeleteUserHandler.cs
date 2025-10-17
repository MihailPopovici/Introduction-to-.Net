using UserManagement.Persistence;

namespace UserManagement.Features.Users;

public class DeleteUserHandler(UserManagementContext dbContext)
{
    private readonly UserManagementContext _dbContext = dbContext;
    
    public async Task<IResult> Handle(DeleteUserRequest request)
    {
        var user = await _dbContext.Users.FindAsync(request.Id);
        if (user == null)
        {
            return Results.NotFound();
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return Results.NoContent();
    }
}