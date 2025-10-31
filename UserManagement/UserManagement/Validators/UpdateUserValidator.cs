using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserManagement.Features.Users;
using UserManagement.Persistence;

namespace UserManagement.Validators;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator(UserManagementContext context)
    {
        //create a custom rule to check if the user with the given Id exists in the database

        RuleFor(x => x.Id)
            .NotNull().NotEmpty()
            .NotEqual(Guid.Empty).WithMessage("Id must be a valid non-empty GUID.")
            .MustAsync(async (id, cancellation) =>
            {
                if (id == Guid.Empty) return false;
                return await context.Users.AnyAsync(u => u.Id == id, cancellation);
            })
            .WithMessage("User with the specified Id does not exist.");

       
        RuleFor(x=>x.FullName).NotEmpty().NotNull().MinimumLength(3).WithMessage("Fullname must be at least 3 characters long.");
        RuleFor(x => x.Email)
            .NotNull().NotEmpty()
            .EmailAddress().WithMessage("A valid email is required.")
            // Ensure email is unique excluding the current user being updated
            .MustAsync(async (request, email, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(email)) return true; // already handled by NotEmpty
                return !await context.Users.AnyAsync(u => u.Email == email && u.Id != request.Id, cancellation);
            })
            .WithMessage("Email is already in use by another user.");

    }
}