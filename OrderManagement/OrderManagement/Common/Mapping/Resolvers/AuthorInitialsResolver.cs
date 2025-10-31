using AutoMapper;
using OrderManagement.Features;
using OrderManagement.Features.Orders;

namespace OrderManagement.Common.Mapping.Resolvers;

public class AuthorInitialsResolver : IValueResolver<Order, OrderProfileDTO, string>
{
    public string Resolve(Order source, OrderProfileDTO destination, string destMember, ResolutionContext context)
    {
        var author = (source.Author ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(author))
            return "?";

        var parts = author.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
            return parts[0].Length > 0 ? char.ToUpperInvariant(parts[0][0]).ToString() : "?";

        var first = parts[0];
        var last = parts[parts.Length - 1];
        var firstInitial = first.Length > 0 ? char.ToUpperInvariant(first[0]) : '?';
        var lastInitial = last.Length > 0 ? char.ToUpperInvariant(last[0]) : '?';
        return $"{firstInitial}{lastInitial}";
    }
}