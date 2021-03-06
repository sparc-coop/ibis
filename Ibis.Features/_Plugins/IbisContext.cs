using Microsoft.EntityFrameworkCore;

namespace Ibis.Features._Plugins;

public class IbisContext : DbContext
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<User> Users => Set<User>();

    public IbisContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>().HasPartitionKey(x => x.UserId);
        builder.Entity<Room>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId);
        builder.Entity<Message>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId);
    }
}
