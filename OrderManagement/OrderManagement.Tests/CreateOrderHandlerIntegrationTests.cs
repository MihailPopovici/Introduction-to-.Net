using System.Globalization;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderManagement.Common.Mapping;
using OrderManagement.Common.Logging;
using OrderManagement.Data;
using OrderManagement.Features.Orders;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace OrderManagement.Tests
{
    public class CreateOrderHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        private readonly MemoryCache _cache;
        private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
        private readonly IValidator<CreateOrderProfileRequest> _validator;
        private readonly CreateOrderHandler _handler;

        public CreateOrderHandlerIntegrationTests()
        {
            _context = new ApplicationContext();
            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OrderMappingProfile>();
                cfg.AddProfile<AdvancedOrderMappingProfile>();
            });
            _mapper = config.CreateMapper();

            _cache = new MemoryCache(new MemoryCacheOptions());

            _loggerMock = new Mock<ILogger<CreateOrderHandler>>();
            
            _validator = new CreateOrderProfileValidator(_context, NullLogger<CreateOrderProfileValidator>.Instance);

            _handler = new CreateOrderHandler(_context, _mapper, _cache, _validator, _loggerMock.Object);
        }

        public void Dispose()
        {
            _cache.Dispose();
        }

        [Fact]
        public async Task Handle_ValidTechnicalOrderRequest_CreatesOrderWithCorrectMappings()
        {
            // Arrange
            var request = new CreateOrderProfileRequest
            {
                Title = "Mastering APIs",
                Author = "Jane Doe",
                ISBN = "978-1234567897",
                Category = OrderCategory.Technical,
                Price = 49.99m,
                PublishedDate = DateTime.UtcNow.AddMonths(-6),
                CoverImageUrl = "https://example.com/cover.jpg",
                StockQuantity = 10
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Technical & Professional", result.CategoryDisplayName);
            Assert.Equal("JD", result.AuthorInitials);
            Assert.Contains("month", result.PublishedAge, StringComparison.OrdinalIgnoreCase);

            var currencySymbol = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
            Assert.StartsWith(currencySymbol, result.FormattedPrice);

            Assert.Equal("In Stock", result.AvailabilityStatus);

            // Verify OrderCreationStarted logged at least once
            var loggedStarted = _loggerMock.Invocations.Any(inv =>
                inv.Method.Name == "Log" &&
                inv.Arguments.Count > 1 &&
                inv.Arguments[1] is EventId ev &&
                ev.Id == LogEvents.OrderCreationStarted);
            Assert.True(loggedStarted, "Expected OrderCreationStarted event to be logged.");
        }

        [Fact]
        public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
        {
            // Arrange: existing order
            var existing = new Order
            {
                Title = "Existing Book",
                Author = "Some Author",
                ISBN = "1112223334",
                Category = OrderCategory.Fiction,
                Price = 10m,
                PublishedDate = DateTime.UtcNow.AddYears(-1),
                StockQuantity = 5,
                CreatedAt = DateTime.UtcNow
            };
            _context.Orders.Add(existing);

            var request = new CreateOrderProfileRequest
            {
                Title = "New Book",
                Author = "New Author",
                ISBN = "1112223334",
                Category = OrderCategory.NonFiction,
                Price = 15m,
                PublishedDate = DateTime.UtcNow.AddYears(-1),
                StockQuantity = 3
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));
            // Prefer checking the ValidationException errors to ensure ISBN is reported
            Assert.True(ex.Errors.Any(err => string.Equals(err.PropertyName, "ISBN", StringComparison.OrdinalIgnoreCase)),
                $"Expected validation errors to include ISBN, got: {ex.Message}");

            var loggedValidation = _loggerMock.Invocations.Any(inv =>
                inv.Method.Name == "Log" &&
                inv.Arguments.Count > 1 &&
                inv.Arguments[1] is EventId ev &&
                ev.Id == LogEvents.OrderValidationFailed);
            Assert.True(loggedValidation, "Expected OrderValidationFailed event to be logged.");
        }

        [Fact]
        public async Task Handle_ChildrensOrderRequest_AppliesDiscountAndConditionalMapping()
        {
            // Arrange
            var request = new CreateOrderProfileRequest
            {
                Title = "Adventures for Kids",
                Author = "Alice Child",
                ISBN = "999-8887776661",
                Category = OrderCategory.Children,
                Price = 20m,
                PublishedDate = DateTime.UtcNow.AddMonths(-2),
                CoverImageUrl = "https://example.com/child.jpg",
                StockQuantity = 4
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal("Children's Orders", result.CategoryDisplayName);
            Assert.Equal(18.00m, result.Price); // 10% discount applied
            Assert.Null(result.CoverImageUrl);
        }
    }
}
