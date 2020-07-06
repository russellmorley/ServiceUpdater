using Microsoft.EntityFrameworkCore;
using Common;

namespace ServiceManager.Models
{
    public class ServiceManagerContext : DbContext
    {
        public ServiceManagerContext(DbContextOptions<ServiceManagerContext> options)
            : base(options)
        {
        }

        public DbSet<InformItem> InformItems { get; set; }
        public DbSet<Deploy> Deploys { get; set; }

        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Deploy>()
                .HasMany(d => d.InformItems) //configure navigation property Deploy.InformItems
                .WithOne();

            modelBuilder.Entity<InformItem>()
                .HasOne<Deploy>()   // no navigation property
                .WithMany()
                .HasForeignKey(i => i.DeployId); // configure foreign key.
        }
        */
    }
}
