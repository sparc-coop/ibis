using Microsoft.EntityFrameworkCore;

namespace Ibis.Features._Plugins
{
    public class IbisContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }


        public IbisContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
           
            builder.Entity<Project>().HasPartitionKey(x => x.Id);

        }
    }
}
