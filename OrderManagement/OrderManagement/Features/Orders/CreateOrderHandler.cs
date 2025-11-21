using System.Diagnostics;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using OrderManagement.Common.Logging;
using OrderManagement.Data;

namespace OrderManagement.Features.Orders
{
    public class CreateOrderHandler(
        ApplicationContext context,
        IMapper mapper,
        IMemoryCache cache,
        IValidator<CreateOrderProfileRequest> validator,
        ILogger<CreateOrderHandler> logger)
        : IRequestHandler<CreateOrderProfileRequest, OrderProfileDTO>
    {
        private readonly ApplicationContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        private readonly IValidator<CreateOrderProfileRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        private readonly ILogger<CreateOrderHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private const string AllOrdersCacheKey = "all_orders";

        public async Task<OrderProfileDTO> Handle(CreateOrderProfileRequest request, CancellationToken cancellationToken)
        {
            var operationId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var overallStopwatch = Stopwatch.StartNew();

            using (_logger.BeginScope(new System.Collections.Generic.Dictionary<string, object>
                   {
                       ["OperationId"] = operationId,
                       ["CorrelationId"] = "" 
                   }))
            {
                _logger.LogInformation(new EventId(LogEvents.OrderCreationStarted),
                    "Order creation started: Title='{Title}', Author='{Author}', Category='{Category}', ISBN='{ISBN}'",
                    request.Title, request.Author, request.Category, request.ISBN);
                
                var validationStopwatch = Stopwatch.StartNew();
                try
                {
                    await _validator.ValidateAndThrowAsync(request, cancellationToken);
                }
                catch (ValidationException vex)
                {
                    validationStopwatch.Stop();

                    _logger.LogWarning(new EventId(LogEvents.OrderValidationFailed),
                        "Order validation failed for OperationId={OperationId}, Title='{Title}', ISBN='{ISBN}', Category={Category}: {ErrorMessage}",
                        operationId, request.Title, request.ISBN, request.Category, vex.Message);

                    var totalDuration = overallStopwatch.Elapsed;

                    var errorMetrics = new OrderCreationMetrics(
                        operationId,
                        request.Title,
                        request.ISBN,
                        request.Category,
                        validationStopwatch.Elapsed,
                        TimeSpan.Zero,
                        totalDuration,
                        false,
                        vex.Message);

                    _logger.LogOrderCreationMetrics(errorMetrics);

                    throw;
                }
                validationStopwatch.Stop();
                
                _logger.LogInformation(new EventId(LogEvents.ISBNValidationPerformed),
                    "ISBN validation performed for OperationId={OperationId}, ISBN='{ISBN}'",
                    operationId, request.ISBN);
                
                var isbnCheckStopwatch = Stopwatch.StartNew();
                var exists = _context.Orders.Any(o => o.ISBN == request.ISBN);
                isbnCheckStopwatch.Stop();

                if (exists)
                {
                    _logger.LogWarning(new EventId(LogEvents.OrderValidationFailed),
                        "Order creation failed - duplicate ISBN for OperationId={OperationId}, ISBN='{ISBN}'",
                        operationId, request.ISBN);

                    var totalDuration = overallStopwatch.Elapsed;
                    var metrics = new OrderCreationMetrics(
                        operationId,
                        request.Title,
                        request.ISBN,
                        request.Category,
                        validationStopwatch.Elapsed + isbnCheckStopwatch.Elapsed,
                        TimeSpan.Zero,
                        totalDuration,
                        false,
                        "Duplicate ISBN");

                    _logger.LogOrderCreationMetrics(metrics);

                    throw new ValidationException($"Order with ISBN '{request.ISBN}' already exists.");
                }
                
                _logger.LogInformation(new EventId(LogEvents.StockValidationPerformed),
                    "Stock validation performed for OperationId={OperationId}, StockQuantity={Stock}",
                    operationId, request.StockQuantity);

                if (request.StockQuantity < 0)
                {
                    _logger.LogWarning(new EventId(LogEvents.OrderValidationFailed),
                        "Order creation failed - invalid stock for OperationId={OperationId}, StockQuantity={Stock}",
                        operationId, request.StockQuantity);

                    var totalDuration = overallStopwatch.Elapsed;
                    var metrics = new OrderCreationMetrics(
                        operationId,
                        request.Title,
                        request.ISBN,
                        request.Category,
                        validationStopwatch.Elapsed,
                        TimeSpan.Zero,
                        totalDuration,
                        false,
                        "Invalid stock quantity");

                    _logger.LogOrderCreationMetrics(metrics);

                    throw new ValidationException("Stock quantity cannot be negative.");
                }
                
                var order = _mapper.Map<Order>(request);
                
                var dbStopwatch = Stopwatch.StartNew();
                _logger.LogInformation(new EventId(LogEvents.DatabaseOperationStarted),
                    "Database operation started for OperationId={OperationId}, ISBN='{ISBN}'",
                    operationId, request.ISBN);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(cancellationToken);

                dbStopwatch.Stop();
                _logger.LogInformation(new EventId(LogEvents.DatabaseOperationCompleted),
                    "Database operation completed for OperationId={OperationId}, OrderId={OrderId}, DurationMs={DurationMs}",
                    operationId, order.Id, (long)dbStopwatch.Elapsed.TotalMilliseconds);
                
                _cache.Remove(AllOrdersCacheKey);
                _logger.LogInformation(new EventId(LogEvents.CacheOperationPerformed),
                    "Cache operation performed for OperationId={OperationId}, Key={CacheKey}",
                    operationId, AllOrdersCacheKey);

                overallStopwatch.Stop();

                var metricsSuccess = new OrderCreationMetrics(
                    operationId,
                    request.Title,
                    request.ISBN,
                    request.Category,
                    validationStopwatch.Elapsed + isbnCheckStopwatch.Elapsed,
                    dbStopwatch.Elapsed,
                    overallStopwatch.Elapsed,
                    true,
                    null);

                _logger.LogOrderCreationMetrics(metricsSuccess);

                _logger.LogInformation(new EventId(LogEvents.OrderCreationCompleted),
                    "Order created successfully for OperationId={OperationId}, OrderId={OrderId}, ISBN='{ISBN}'",
                    operationId, order.Id, order.ISBN);

                return _mapper.Map<OrderProfileDTO>(order);
            }
        }
    }
}
