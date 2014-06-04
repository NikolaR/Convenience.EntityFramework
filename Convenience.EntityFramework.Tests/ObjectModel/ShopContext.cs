using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework.Tests.ObjectModel
{
    public class ShopContext : DbContext
    {
        public ShopContext()
            : base("Default")
        { }

        public DbSet<Customer> Customer { get; set; }

        public DbSet<Order> Order { get; set; }

        public DbSet<LineItem> LineItems { get; set; }

        public override int SaveChanges()
        {
            var now = DateTime.Now;
            foreach (var dbEntityEntry in ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified))
            {
                var entity = dbEntityEntry.Entity as Entity;
                if (entity != null)
                    entity.LastUpdateTs = now;
            }
            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var customer = modelBuilder.Entity<Customer>()
                .ToTable("Customer");
            customer.HasKey(t => t.Id);
            customer.Property(t => t.LastUpdateTs)
                .IsConcurrencyToken();
            customer.Property(t => t.Name);
            customer.HasMany(t => t.Orders);

            var order = modelBuilder.Entity<Order>()
                .ToTable("Order");
            order.HasKey(t => t.Id);
            order.Property(t => t.LastUpdateTs)
                .IsConcurrencyToken();
            order.Property(t => t.OrderTime);
            order.HasMany(t => t.Items);
            order.HasOptional(t => t.Customer)
                .WithMany(c => c.Orders);

            var item = modelBuilder.Entity<LineItem>()
                .ToTable("LineItem");
            item.HasKey(t => t.Id);
            item.Property(t => t.LastUpdateTs)
                .IsConcurrencyToken();
            item.Property(t => t.Name);
            item.Property(t => t.Price);
            item.HasOptional(t => t.Order)
                .WithMany(o => o.Items);

            base.OnModelCreating(modelBuilder);
        }
    }
}
