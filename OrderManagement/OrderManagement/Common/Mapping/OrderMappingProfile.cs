using AutoMapper;
using OrderManagement.Features.Orders;
using OrderManagement.Features;

namespace OrderManagement.Common.Mapping
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            // Simple mappings to complement AdvancedOrderMappingProfile
            CreateMap<CreateOrderProfileRequest, Order>();
            CreateMap<Order, OrderProfileDTO>();
        }
    }
}

