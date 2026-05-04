using Microsoft.EntityFrameworkCore;
using Shop.Users.Models;

namespace Shop.Users.Data;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
}