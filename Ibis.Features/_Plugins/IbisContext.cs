using Ibis.Features.Conversations.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ibis.Features._Plugins
{
    public class IbisContext : DbContext
    {
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<Message> Messages => Set<Message>();

        public IbisContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Project>().HasPartitionKey(x => x.Id);
            builder.Entity<Conversation>().ToContainer("Conversations").HasPartitionKey(x => x.ConversationId);
            builder.Entity<Message>().ToContainer("Conversations").HasPartitionKey(x => x.ConversationId);
        }
    }
}
