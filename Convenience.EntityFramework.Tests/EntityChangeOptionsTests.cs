using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Convenience.EntityFramework.Tests.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Convenience.EntityFramework.Tests
{
    [TestClass]
    public class EntityChangeOptionsTests : DbTestBase
    {
        [TestMethod]
        public void basic()
        {
            var opts = EfWriter.ChangesFrom(new Customer());
            opts.Include("Orders.Items");

            EntityChangeOptions<Customer> cust;
            var doInclude = opts.ShouldIncludeNavigation("Orders", out cust);
            Assert.IsTrue(doInclude);

            doInclude = cust.ShouldIncludeNavigation("Items");
            Assert.IsTrue(doInclude);

            doInclude = cust.ShouldIncludeNavigation("Items2");
            Assert.IsFalse(doInclude);
        }

        [TestMethod]
        public void basic2()
        {
            var opts = EfWriter.ChangesFrom(new Customer());
            opts.Include(c => c.Orders.Select(o => o.Items));

            EntityChangeOptions<Customer> cust;
            var doInclude = opts.ShouldIncludeNavigation("Orders", out cust);
            Assert.IsTrue(doInclude);

            doInclude = cust.ShouldIncludeNavigation("Items");
            Assert.IsTrue(doInclude);

            doInclude = cust.ShouldIncludeNavigation("Items2");
            Assert.IsFalse(doInclude);
        }

        [TestMethod]
        public void test3()
        {
            Order order = new Order(DateTime.Now)
            {
                Items = new List<LineItem>()
                {
                    new LineItem("cheese", 10),
                    new LineItem("ham", 20),
                    new LineItem("dough", 5)
                },
                Customer = new Customer("Nikola")
            };
            Db.Order.Add(order);
            Db.SaveChanges();
            var itemCount = Db.LineItems.Count();
            Assert.AreEqual(itemCount, 3);

            var dough = Db.LineItems.Find(order.Items[2].Id);
            Assert.AreEqual(dough.Name, "dough");


            var orderDto = SerializationUtils.DeepClone(order);
            orderDto.Items[2].Name = "Pizza dough";
            orderDto.Customer.Name = "Petar";
            EfWriter.ChangesFrom(orderDto).Include(o => o.Items).Apply();
            Db.SaveChanges();
            ReinitDb();
            order = Db.Order.Find(order.Id);
            Assert.AreEqual(order.Items[2].Name, "Pizza dough");
            Assert.AreEqual(order.Customer.Name, "Nikola");

            orderDto = SerializationUtils.DeepClone(order);
            orderDto.Items[2].Name = "Special pizza dough!";
            orderDto.Customer.Name = "Milos";
            EfWriter.ChangesFrom(orderDto).Include(o => o.Customer).Apply();
            Db.SaveChanges();
            ReinitDb();
            order = Db.Order.Find(order.Id);
            Assert.AreEqual(order.Items[2].Name, "Pizza dough");
            Assert.AreEqual(order.Customer.Name, "Milos");
        }
    }
}
