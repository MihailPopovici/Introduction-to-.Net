using OrderManagement.Features;
using OrderManagement.Features.Orders;

namespace OrderManagement.Common.Logging;

public record OrderCreationMetrics(
    string OperationId,
    string OrderTitle,
    string ISBN,
    OrderCategory Category,
    TimeSpan ValidationDuration,
    TimeSpan DatabaseSaveDuration,
    TimeSpan TotalDuration,
    bool Success,
    string? ErrorReason = null
);