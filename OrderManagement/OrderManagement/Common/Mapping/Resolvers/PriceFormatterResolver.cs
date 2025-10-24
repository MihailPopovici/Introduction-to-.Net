using System.Globalization;
using AutoMapper;
using OrderManagement.Features;

namespace OrderManagement.Common.Mapping.Resolvers;

public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDTO, string>
{
    public string Resolve(Order source, OrderProfileDTO destination, string destMember, ResolutionContext context)
    {
        var displayPrice = source.Category == OrderCategory.Children ? source.Price * 0.9m : source.Price;
        return displayPrice.ToString("C2", CultureInfo.CurrentCulture);
    }
}