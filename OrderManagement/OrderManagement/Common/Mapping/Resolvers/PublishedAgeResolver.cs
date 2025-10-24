using AutoMapper;
using OrderManagement.Features;

namespace OrderManagement.Common.Mapping.Resolvers;

public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDTO, string>
{
    public string Resolve(Order source, OrderProfileDTO destination, string destMember, ResolutionContext context)
    {
        var published = source.PublishedDate;
        var now = DateTime.UtcNow;
        var age = now - published;

        if (age.TotalDays < 30)
            return "New Release";

        if (age.TotalDays < 365)
        {
            var months = (int)Math.Floor(age.TotalDays / 30.0);
            return $"{months} month{(months == 1 ? "" : "s")} old";
        }

        if (age.TotalDays < 1825) 
        {
            var years = (int)Math.Floor(age.TotalDays / 365.0);
            return $"{years} year{(years == 1 ? "" : "s")} old";
        }

        return "Classic";
    }
}