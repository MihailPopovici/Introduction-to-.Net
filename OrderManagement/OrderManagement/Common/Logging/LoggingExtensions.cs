using Microsoft.Extensions.Logging;

namespace OrderManagement.Common.Logging
{
    public static class LoggingExtensions
    {
        public static void LogOrderCreationMetrics(this ILogger logger, OrderCreationMetrics metrics)
        {
            var eventId = metrics.Success
                ? new EventId(LogEvents.OrderCreationCompleted)
                : new EventId(LogEvents.OrderValidationFailed);

            var message = "Order creation metrics: OperationId={OperationId}, Title='{Title}', ISBN='{ISBN}', Category={Category}, ValidationDurationMs={ValidationMs}, DatabaseSaveDurationMs={DatabaseMs}, TotalDurationMs={TotalMs}, Success={Success}, ErrorReason={ErrorReason}";

            if (metrics.Success)
            {
                logger.LogInformation(eventId, message,
                    metrics.OperationId,
                    metrics.OrderTitle,
                    metrics.ISBN,
                    metrics.Category,
                    (long)metrics.ValidationDuration.TotalMilliseconds,
                    (long)metrics.DatabaseSaveDuration.TotalMilliseconds,
                    (long)metrics.TotalDuration.TotalMilliseconds,
                    metrics.Success,
                    metrics.ErrorReason);
            }
            else
            {
                logger.LogWarning(eventId, message,
                    metrics.OperationId,
                    metrics.OrderTitle,
                    metrics.ISBN,
                    metrics.Category,
                    (long)metrics.ValidationDuration.TotalMilliseconds,
                    (long)metrics.DatabaseSaveDuration.TotalMilliseconds,
                    (long)metrics.TotalDuration.TotalMilliseconds,
                    metrics.Success,
                    metrics.ErrorReason);
            }
        }
    }
}

