using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Persistence;
using UserManagement.Validators;

namespace UserManagement.Features.Users;

public class UpdateUserHandler(UserManagementContext dbContext, IMapper mapper)
{ 
    //Todo - discuss later on
    public async Task<IResult> Handle(UpdateUserRequest request)
    {
        var validator = new UpdateUserValidator(dbContext);
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
        
        var updatedUser = mapper.Map<User>(request);
        
        //TODO - discuss later on 
        dbContext.Remove(existingUser);
        dbContext.Add(updatedUser);
        await dbContext.SaveChangesAsync();

        return Results.Ok(updatedUser);
    }
}