using Microsoft.EntityFrameworkCore;
using Shop.Users.Models;

namespace Shop.Users.Data;

public class UsersDbContext (DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}