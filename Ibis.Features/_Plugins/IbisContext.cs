using MediatR;
using Microsoft.EntityFrameworkCore;
using Sparc.Database.Cosmos;

namespace Ibis.Features._Plugins;

public class IbisContext : DbContextWithEvents
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<User> Users => Set<User>();

    public IbisContext(DbContextOptions options, IMediator mediator) : base(options, mediator)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>().HasPartitionKey(x => x.UserId);
        builder.Entity<Room>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId);
        builder.Entity<Message>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId);
    }
}
