namespace UserManagement.Exceptions;

public class UserNotFoundException : BaseException
{
    protected UserNotFoundException(Guid userId) : base($"User with ID {userId} was not found", 404, "USER_NOT_FOUND")
    {
        
    }
}