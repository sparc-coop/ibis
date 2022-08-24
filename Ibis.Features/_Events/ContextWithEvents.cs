using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Sparc.Database.Cosmos;

public class DbContextWithEvents : DbContext
{
    public DbContextWithEvents(DbContextOptions options, IMediator mediator) : base(options)
    {
        Mediator = mediator;
    }

    public IMediator Mediator { get; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    async Task DispatchDomainEventsAsync()
    {
        var domainEntities = ChangeTracker.Entries<IRootWithEvents>().Where(x => x.Entity.Events != null && x.Entity.Events.Any());
        var domainEvents = domainEntities.SelectMany(x => x.Entity.Events!).ToList();
        domainEntities.ToList().ForEach(entity => entity.Entity.Events!.Clear());

        var tasks = domainEvents
            .Select(async (domainEvent) => {
                await Mediator.Publish(domainEvent);
            });

        await Task.WhenAll(tasks);
    }
}
