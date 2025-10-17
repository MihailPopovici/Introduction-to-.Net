using FluentValidation;
using BookManagement.Features.Books;

namespace BookManagement.Validators;

public class UpdateBookValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().NotNull().MinimumLength(1).WithMessage("Title is required.");
        RuleFor(x => x.Author).NotEmpty().NotNull().MinimumLength(2).WithMessage("Author is required.");
        RuleFor(x => x.Year).InclusiveBetween(0, DateTime.UtcNow.Year).WithMessage("Year must be valid.");
    }
}