using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ibis.Features._Plugins;

public class IbisContext : SparcContext
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<User> Users => Set<User>();

    public IbisContext(DbContextOptions options, IMediator mediator) : base(options, mediator)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>().Ignore(x => x.Events);
        builder.Entity<Room>().Ignore(x => x.Events);
        builder.Entity<Message>().Ignore(x => x.Events);

        builder.Entity<User>().ToContainer("Users").HasPartitionKey(x => x.UserId);
        builder.Entity<Room>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId);
        builder.Entity<Message>().ToContainer("Rooms").HasPartitionKey(x => x.RoomId);
    }
}
