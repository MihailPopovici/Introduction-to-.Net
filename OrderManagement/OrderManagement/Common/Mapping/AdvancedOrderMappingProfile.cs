using System;
using AutoMapper;
using OrderManagement.Features;
using OrderManagement.Common.Mapping.Resolvers;

namespace OrderManagement.Common.Mapping
{
    public class AdvancedOrderMappingProfile : Profile
    {
        public AdvancedOrderMappingProfile()
        {
            // CreateOrderProfileRequest -> Order
            CreateMap<CreateOrderProfileRequest, Order>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                //.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.StockQuantity > 0));
                //.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
                

            // Order -> OrderProfileDTO using resolvers
            CreateMap<Order, OrderProfileDTO>()
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src.Category == OrderCategory.Children ? src.Price * 0.9m : src.Price))
                .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
                .ForMember(dest => dest.CoverImageUrl,
                    opt => opt.MapFrom(src => src.Category == OrderCategory.Children ? null : src.CoverImageUrl))
                .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
                .ForMember(dest => dest.PublishedAge, opt => opt.MapFrom<PublishedAgeResolver>())
                .ForMember(dest => dest.AuthorInitials, opt => opt.MapFrom<AuthorInitialsResolver>())
                .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>());
        }
    }
}