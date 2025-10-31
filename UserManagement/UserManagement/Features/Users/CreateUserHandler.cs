using UserManagement.Persistence;
using UserManagement.Validators;

namespace UserManagement.Features.Users;

public class CreateUserHandler(UserManagementContext context, ILogger<CreateUserHandler> logger)
{
    public async Task<IResult> Handle(CreateUserRequest request)
    {
        logger.LogInformation("Creating a new user with Name: {FullName} and Email: {Email}", request.Email, request.FullName);
        //Todo create a middleware for validation
        // var validator = new CreateUserValidator();
        // var validationResult = await validator.ValidateAsync(request);
        // if (!validationResult.IsValid)
        // { 
        //     return Results.BadRequest(validationResult.Errors);
        // }
        var user = new User(Guid.NewGuid(), request.FullName, request.Email);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        logger.LogInformation("User created successfully with Id: {UserId}", user.Id);
        
        return Results.Created($"/users/{user.Id}", user);
    }
    
}