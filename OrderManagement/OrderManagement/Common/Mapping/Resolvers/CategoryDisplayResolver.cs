using AutoMapper;
using OrderManagement.Features;
using OrderManagement.Features.Orders;
namespace OrderManagement.Common.Mapping.Resolvers;

public class CategoryDisplayResolver : IValueResolver<Order, OrderProfileDTO, string>
{
    public string Resolve(Order source, OrderProfileDTO destination, string destMember, ResolutionContext context)
    {
        return source.Category switch
        {
            OrderCategory.Fiction => "Fiction & Literature",
            OrderCategory.NonFiction => "Non-Fiction",
            OrderCategory.Technical => "Technical & Professional",
            OrderCategory.Children => "Children's Orders",
            _ => "Uncategorized"
        };
    }
}