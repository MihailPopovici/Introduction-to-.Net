using OrderManagement.Features.Orders;

namespace OrderManagement.Data
{
    public class ApplicationContext
    {
        public List<Order> Orders { get; } = [];

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}

