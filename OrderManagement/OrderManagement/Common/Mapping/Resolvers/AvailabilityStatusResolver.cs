using AutoMapper;
using OrderManagement.Features;
using OrderManagement.Features.Orders;

namespace OrderManagement.Common.Mapping.Resolvers;

public class AvailabilityStatusResolver : IValueResolver<Order, OrderProfileDTO, string>
{
    public string Resolve(Order source, OrderProfileDTO destination, string destMember, ResolutionContext context)
    {
        if (!source.IsAvailable)
            return "Out of Stock";

        var qty = source.StockQuantity;

        if (qty == 0)
            return "Unavailable";

        if (qty == 1)
            return "Last Copy";

        if (qty <= 5)
            return "Limited Stock";

        return "In Stock";
    }
}