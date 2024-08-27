using Microsoft.EntityFrameworkCore;

namespace Ibis._Plugins;

public class IbisContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>().ToContainer("Users").HasPartitionKey(x => x.UserId);
        builder.Entity<Room>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId);
        builder.Entity<Message>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId).HasQueryFilter(x => x.DeletedDate == null);
        builder.Entity<UserCharge>().ToContainer("Users").HasPartitionKey(x => x.UserId);
    }
}
