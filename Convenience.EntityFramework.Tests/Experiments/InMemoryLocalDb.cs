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

        [TestMethod]
        public void random_test()
        {
            EfMetaUtils meta = new EfMetaUtils(Db);
            var navProps = meta.GetNavigationProperties(typeof(Customer));
        }

        [TestMethod]
        public void test2()
        {
            var pera = new Customer("Pera");
            Db.Customer.Add(pera);
            Db.SaveChanges();
            Assert.AreNotEqual(pera.Id, 0);

            var newPera = new Customer("Pera Peric") { Id = pera.Id };
            EfWriter.ApplyEntityChanges(newPera);
            Db.SaveChanges();

            ReinitDb();
            pera = Db.Customer.Find(pera.Id);
            Assert.AreEqual(pera.Name, "Pera Peric");
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
                }
            };
            Db.Order.Add(order);
            Db.SaveChanges();
            var itemCount = Db.LineItems.Count();
            Assert.AreEqual(itemCount, 3);
            
            var dough = Db.LineItems.Find(order.Items[2].Id);
            Assert.AreEqual(dough.Name, "dough");


            var orderDto = SerializationUtils.DeepClone(order);
            orderDto.Items[2].Name = "Pizza dough";
            EfWriter.ApplyGraphChanges<Order, LineItem>(orderDto);
            Db.SaveChanges();
            ReinitDb();
            dough = Db.LineItems.Find(order.Items[2].Id);
            Assert.AreEqual(dough.Name, "Pizza dough");

            orderDto = SerializationUtils.DeepClone(order);
            orderDto.Items[2].Name = "Special pizza dough!";
            EfWriter.ApplyGraphChanges<Order, Customer>(orderDto);
            Db.SaveChanges();
            ReinitDb();
            dough = Db.LineItems.Find(order.Items[2].Id);
            Assert.AreEqual(dough.Name, "Pizza dough");
        }
    }
}
