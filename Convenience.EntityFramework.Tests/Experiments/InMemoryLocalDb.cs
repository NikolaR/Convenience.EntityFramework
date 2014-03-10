using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Convenience.EntityFramework.Tests.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Convenience.EntityFramework.Tests.Experiments
{
    [TestClass]
    public class InMemoryLocalDb : DbTestBase
    {
        [TestMethod]
        public void is_in_memory()
        {
            ShopContext ctx;
            using (ctx = new ShopContext())
            {
                ctx.Customer.Add(new Customer("Nikola"));
                ctx.SaveChanges();
            }
            using (ctx = new ShopContext())
                Assert.AreEqual(ctx.Customer.Count(), 1);

            using (ctx = new ShopContext())
            {
                ctx.Customer.Add(new Customer("Petar"));
                ctx.SaveChanges();
            }
            using (ctx = new ShopContext())
                Assert.AreEqual(ctx.Customer.Count(), 2);
        }
    }
}
