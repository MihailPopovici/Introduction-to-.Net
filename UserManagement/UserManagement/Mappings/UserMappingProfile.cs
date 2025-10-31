using AutoMapper;
using UserManagement.Features.Users;

namespace UserManagement.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, CreateUserRequest>();
        
        CreateMap<CreateUserRequest, User>()
            .ConstructUsing(src => new User(Guid.NewGuid(), src.FullName, src.Email));

        CreateMap<UpdateUserRequest, User>()
            .ConstructUsing(src => new User(src.Id, src.FullName, src.Email));
    }
    
}