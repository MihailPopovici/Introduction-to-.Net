using Microsoft.EntityFrameworkCore;
using UserManagement.Features.Users;

namespace UserManagement.Persistence;

public class UserManagementContext(DbContextOptions<UserManagementContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}