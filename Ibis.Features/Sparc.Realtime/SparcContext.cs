using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Sparc.Core;

public class SparcContext : DbContext
{
    public SparcContext(DbContextOptions options, IMediator mediator) : base(options)
    {
        Mediator = mediator;
    }

    public IMediator Mediator { get; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync();
        return result; 
    }

    async Task DispatchDomainEventsAsync()
    {
        var domainEntities = ChangeTracker.Entries<ISparcRoot>().Where(x => x.Entity.Events != null && x.Entity.Events.Any());
        var domainEvents = domainEntities.SelectMany(x => x.Entity.Events!).ToList();
        domainEntities.ToList().ForEach(entity => entity.Entity.Events!.Clear());

        var tasks = domainEvents
            .Select(async (domainEvent) => {
                await Mediator.Publish(domainEvent);
            });

        await Task.WhenAll(tasks);
    }
}
