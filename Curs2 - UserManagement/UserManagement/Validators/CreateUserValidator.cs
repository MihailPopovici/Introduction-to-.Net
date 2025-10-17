using FluentValidation;
using UserManagement.Features.Users;

namespace UserManagement.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x=>x.FullName).NotEmpty().NotNull().MinimumLength(3).WithMessage("Fullname must be at least 3 characters long.");
        RuleFor(x=>x.Email).NotEmpty().NotNull().EmailAddress().WithMessage("A valid email is required.");
        
    }
}