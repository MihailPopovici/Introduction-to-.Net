using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace OrderManagement.Common.Middleware
{
    public class CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        private const string CorrelationHeader = "X-Correlation-ID";

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers.ContainsKey(CorrelationHeader)
                ? context.Request.Headers[CorrelationHeader].ToString()
                : System.Guid.NewGuid().ToString("N").Substring(0, 8);

            
            if (!context.Response.HasStarted)
            {
                context.Response.Headers[CorrelationHeader] = correlationId;
            }
            
            context.Items[CorrelationHeader] = correlationId;

            using (logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            }))
            {
                await next(context);
            }
        }
    }
}

