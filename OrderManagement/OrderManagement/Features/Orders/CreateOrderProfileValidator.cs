using System.Text.RegularExpressions;
using FluentValidation;
using OrderManagement.Data;

namespace OrderManagement.Features.Orders
{
    public class CreateOrderProfileValidator : AbstractValidator<CreateOrderProfileRequest>
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<CreateOrderProfileValidator> _logger;

        private static readonly HashSet<string> InappropriateWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "badword1",
            "badword2",
            "inappropriate"
        };

        private static readonly HashSet<string> RestrictedChildrenWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "violence",
            "adult",
            "explicit"
        };

        private static readonly string[] TechnicalKeywords = new[] { "guide", "tutorial", "programming", "api", "architecture", "design", "patterns", "algorithm", "framework" };

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public CreateOrderProfileValidator(ApplicationContext context, ILogger<CreateOrderProfileValidator> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Title rules
            RuleFor(x => x.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Title is required.")
                .MinimumLength(1).WithMessage("Title must be at least 1 character.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
                .Must(BeValidTitle).WithMessage("Title contains inappropriate content.")
                .MustAsync(BeUniqueTitle).WithMessage("Title must be unique for the same author.");

            // Author rules
            RuleFor(x => x.Author)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Author is required.")
                .MinimumLength(2).WithMessage("Author must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Author cannot exceed 100 characters.")
                .Must(BeValidAuthorName).WithMessage("Author name contains invalid characters.");

            // ISBN rules
            RuleFor(x => x.ISBN)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("ISBN is required.")
                .Must(BeValidIsbn).WithMessage("ISBN must be a valid 10- or 13-digit ISBN (hyphens allowed).")
                .MustAsync(BeUniqueIsbn).WithMessage("ISBN must be unique in the system.");

            // Category rules
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Category must be a valid value.");

            // Price rules
            RuleFor(x => x.Price)
                .GreaterThan(0m).WithMessage("Price must be greater than 0.")
                .LessThan(10000m).WithMessage("Price must be less than 10,000.");

            // PublishedDate rules
            RuleFor(x => x.PublishedDate)
                .Must(d => d <= DateTime.UtcNow).WithMessage("Published date cannot be in the future.")
                .Must(d => d.Year >= 1400).WithMessage("Published date cannot be before year 1400.");

            // StockQuantity rules
            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("StockQuantity cannot be negative.")
                .LessThanOrEqualTo(100000).WithMessage("StockQuantity cannot exceed 100000.");

            // CoverImageUrl rules
            When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl), () =>
            {
                RuleFor(x => x.CoverImageUrl)
                    .Must(BeValidImageUrl).WithMessage("CoverImageUrl must be a valid HTTP/HTTPS image URL with an accepted extension.");
            });

            // Conditional rules for Technical orders
            When(x => x.Category == OrderCategory.Technical, () =>
            {
                RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(20m).WithMessage("Technical orders must have a minimum price of $20.00.");

                RuleFor(x => x.Title)
                    .Must(ContainTechnicalKeywords).WithMessage("Technical orders must contain technical keywords in the title.");

                RuleFor(x => x.PublishedDate)
                    .Must(d => d >= DateTime.UtcNow.AddYears(-5)).WithMessage("Technical orders must be published within the last 5 years.");
            });

            // Conditional rules for Children's orders
            When(x => x.Category == OrderCategory.Children, () =>
            {
                RuleFor(x => x.Price)
                    .LessThanOrEqualTo(50m).WithMessage("Children's orders must have a maximum price of $50.00.");

                RuleFor(x => x.Title)
                    .Must(BeAppropriateForChildren).WithMessage("Title contains content not appropriate for children.");
            });

            // Conditional rules for Fiction orders
            When(x => x.Category == OrderCategory.Fiction, () =>
            {
                RuleFor(x => x.Author)
                    .MinimumLength(5).WithMessage("Fiction authors must be at least 5 characters (full name required).");
            });

            // Cross-field validations
            RuleFor(x => x)
                .Must(req => !(req.Price > 100m) || req.StockQuantity <= 20)
                .WithMessage("Expensive orders (>$100) must have stock ≤ 20 units.");

            // Business rules (complex) - run last
            RuleFor(x => x)
                .MustAsync(PassBusinessRules).WithMessage("Order does not satisfy business rules.");
        }

        // Validation methods
        private bool BeValidTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;
            var lowered = title.ToLowerInvariant();
            foreach (var bad in InappropriateWords)
            {
                if (lowered.Contains(bad.ToLowerInvariant()))
                {
                    _logger.LogInformation("Title failed inappropriate words check: '{Title}' contains '{Word}'", title, bad);
                    return false;
                }
            }
            return true;
        }

        private Task<bool> BeUniqueTitle(CreateOrderProfileRequest request, string? title, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(request.Author)) return Task.FromResult(true);

            _logger.LogInformation("Checking title uniqueness for Title='{Title}', Author='{Author}'", title, request.Author);

            var exists = _context.Orders.Any(o =>
                string.Equals(o.Title, title, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(o.Author, request.Author, StringComparison.OrdinalIgnoreCase)
            );

            if (exists)
            {
                _logger.LogWarning("Title is not unique: Title='{Title}', Author='{Author}'", title, request.Author);
            }

            return Task.FromResult(!exists);
        }

        private bool BeValidAuthorName(string? author)
        {
            if (string.IsNullOrWhiteSpace(author)) return false;
            var regex = new Regex("^[\\p{L} .'-]+$", RegexOptions.Compiled);
            var ok = regex.IsMatch(author);
            if (!ok)
            {
                _logger.LogInformation("Author name failed character check: '{Author}'", author);
            }
            return ok;
        }

        private bool BeValidIsbn(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return false;
            var digits = new string(isbn.Where(char.IsDigit).ToArray());
            if (digits.Length == 10 || digits.Length == 13) return true;
            _logger.LogInformation("ISBN format invalid: '{IsbnRaw}' -> digits='{Digits}'", isbn, digits);
            return false;
        }

        private Task<bool> BeUniqueIsbn(string? isbn, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return Task.FromResult(true);
            var digits = new string(isbn.Where(char.IsDigit).ToArray());
            _logger.LogInformation("Checking ISBN uniqueness for digits='{Digits}'", digits);

            var exists = _context.Orders.Any(o => new string(o.ISBN.Where(char.IsDigit).ToArray()) == digits);
            if (exists)
            {
                _logger.LogWarning("ISBN already exists in system: {Isbn}", isbn);
            }
            return Task.FromResult(!exists);
        }

        private bool BeValidImageUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;
            var path = uri.AbsolutePath.ToLowerInvariant();
            var ok = AllowedImageExtensions.Any(ext => path.EndsWith(ext));
            if (!ok)
            {
                _logger.LogInformation("CoverImageUrl failed extension check: '{Url}'", url);
            }
            return ok;
        }

        // Helper: check for technical keywords in the title
        private bool ContainTechnicalKeywords(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;
            var lowered = title.ToLowerInvariant();
            foreach (var kw in TechnicalKeywords)
            {
                if (lowered.Contains(kw.ToLowerInvariant())) return true;
            }
            _logger.LogInformation("Title does not contain technical keywords: '{Title}'", title);
            return false;
        }

        // Helper: check that title is appropriate for children (no restricted words)
        private bool BeAppropriateForChildren(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;
            var lowered = title.ToLowerInvariant();
            foreach (var bad in RestrictedChildrenWords)
            {
                if (lowered.Contains(bad.ToLowerInvariant()))
                {
                    _logger.LogInformation("Title not appropriate for children: '{Title}' contains '{Word}'", title, bad);
                    return false;
                }
            }
            return true;
        }

        private Task<bool> PassBusinessRules(CreateOrderProfileRequest request, CancellationToken ct)
        {
            // Rule 1: Daily order addition limit (max 500 per day)
            var today = DateTime.UtcNow.Date;
            var todaysCount = _context.Orders.Count(o => o.CreatedAt.Date == today);
            _logger.LogInformation("BusinessRule: today's order count = {Count}", todaysCount);
            if (todaysCount >= 500)
            {
                _logger.LogWarning("BusinessRule failed: daily limit reached ({Count})", todaysCount);
                return Task.FromResult(false);
            }

            // Rule 2: Technical orders minimum price ($20.00)
            if (request.Category == OrderCategory.Technical && request.Price < 20m)
            {
                _logger.LogWarning("BusinessRule failed: technical order below minimum price: {Price}", request.Price);
                return Task.FromResult(false);
            }

            // Rule 3: Children's order content restrictions (check Title against restricted words)
            if (request.Category == OrderCategory.Children)
            {
                var titleLower = request.Title.ToLowerInvariant();
                foreach (var restricted in RestrictedChildrenWords)
                {
                    if (titleLower.Contains(restricted.ToLowerInvariant()))
                    {
                        _logger.LogWarning("BusinessRule failed: children's book title contains restricted word '{Word}'", restricted);
                        return Task.FromResult(false);
                    }
                }
            }

            // Rule 4: High-value order stock limit (>$500 = max 10 stock)
            if (request.Price > 500m && request.StockQuantity > 10)
            {
                _logger.LogWarning("BusinessRule failed: high-value order stock exceeds limit. Price={Price}, Stock={Stock}", request.Price, request.StockQuantity);
                return Task.FromResult(false);
            }

            // If all passed
            _logger.LogInformation("All business rules passed for Title='{Title}', ISBN='{Isbn}'", request.Title, request.ISBN);
            return Task.FromResult(true);
        }
    }
}
