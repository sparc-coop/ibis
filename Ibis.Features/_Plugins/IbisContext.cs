using Microsoft.EntityFrameworkCore;

namespace Ibis._Plugins;

public class IbisContext : ApiContext
{
    public IbisContext(
        DbContextOptions options, 
        BlossomNotifier notifier, 
        IHttpContextAccessor http,
        ApiSet<Room> Rooms,
        ApiSet<Message> Messages) : base(options, notifier, http)
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
