using FluentValidation;
using BookManagement.Features.Books;

namespace BookManagement.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty().NotNull().MinimumLength(1).WithMessage("Title is required.");
        RuleFor(x => x.Author).NotEmpty().NotNull().MinimumLength(2).WithMessage("Author is required.");
        RuleFor(x => x.Year).InclusiveBetween(0, DateTime.UtcNow.Year).WithMessage("Year must be valid.");
    }
}