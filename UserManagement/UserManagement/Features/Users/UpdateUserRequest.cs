namespace UserManagement.Features.Users;

public record UpdateUserRequest(Guid Id, string FullName, string Email);