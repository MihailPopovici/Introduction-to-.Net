using UserManagement.Persistence;
using UserManagement.Validators;

namespace UserManagement.Features.Users;

public class CreateUserHandler(UserManagementContext context)
{
    //this is called injection  
    private readonly UserManagementContext _context = context;
    
    public async Task<IResult> Handle(CreateUserRequest request)
    {
        //Todo create a middleware for validation
        var validator = new CreateUserValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {   
            return Results.BadRequest(validationResult.Errors);
        }
        var user = new User(Guid.NewGuid(), request.FullName, request.Email);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Results.Created($"/users/{user.Id}", user);
    }
    
}