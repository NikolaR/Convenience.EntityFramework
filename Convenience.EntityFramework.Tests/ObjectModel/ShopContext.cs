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
        public DbSet<Customer> Customer { get; set; }

        public DbSet<Order> Order { get; set; }

        public DbSet<LineItem> LineItems { get; set; }
    }
}
