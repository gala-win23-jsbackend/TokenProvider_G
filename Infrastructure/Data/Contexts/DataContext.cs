

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Entities;

namespace Infrastructure.Data.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }



}
