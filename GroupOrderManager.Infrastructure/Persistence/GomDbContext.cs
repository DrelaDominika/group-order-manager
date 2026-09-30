using GroupOrderManager.Domain;
using Microsoft.EntityFrameworkCore;

namespace GroupOrderManager.Infrastructure.Persistence;

public class GomDbContext : DbContext
{
    public GomDbContext(DbContextOptions<GomDbContext> options) : base(options)
    {
    }

    public DbSet<GroupOrder> GroupOrders => Set<GroupOrder>();
    public DbSet<GroupOrderItem> GroupOrderItems => Set<GroupOrderItem>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GomDbContext).Assembly);
    }
}