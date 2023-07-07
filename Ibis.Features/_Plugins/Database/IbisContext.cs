using Microsoft.EntityFrameworkCore;

namespace Ibis._Plugins.Database;

public class IbisContext : BlossomContext
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<User> Users => Set<User>();

    public IbisContext(DbContextOptions options, Publisher publisher, IHttpContextAccessor http) : base(options, publisher, http)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>().ToContainer("Users").HasPartitionKey(x => x.UserId);
        builder.Entity<Room>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId);
        builder.Entity<Message>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId).HasQueryFilter(x => x.DeletedDate == null);
        builder.Entity<UserCharge>().ToContainer("Users").HasPartitionKey(x => x.UserId);
    }
}
