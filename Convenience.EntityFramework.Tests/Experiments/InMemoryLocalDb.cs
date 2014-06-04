using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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

        [TestMethod]
        public void referencing_entities_enough_one_way()
        {
            Order order = new Order(DateTime.Now);
            Db.Order.Add(order);
            Db.SaveChanges();

            ReinitDb();
            var item = new LineItem("cheese", 10);
            item.Order = Db.Order.Find(order.Id);
            Db.LineItems.Add(item);
            Db.SaveChanges();

            ReinitDb();
            item = Db.LineItems.Find(item.Id);
            order = Db.Order.Find(order.Id);
            Assert.IsNotNull(item.Order);
            Assert.AreEqual(order.Items.Count, 1);


            order = new Order(DateTime.Now);
            Db.Order.Add(order);
            Db.SaveChanges();

            ReinitDb();
            item = new LineItem("cheese", 10);
            order.Items = new List<LineItem>() { item };
            Db.SaveChanges();

            ReinitDb();
            item = Db.LineItems.Find(item.Id);
            order = Db.Order.Find(order.Id);
            Assert.IsNotNull(item.Order);
            Assert.AreEqual(order.Items.Count, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(DbUpdateConcurrencyException))]
        public void concurrency_token_causes_concurrency_exception_when_updated_and_saved_twice()
        {
            var item = new LineItem()
            {
                Name = "SSD",
                Price = 1.1111111111m
            };
            Db.LineItems.Add(item);
            Db.SaveChanges();

            ReinitDb();
            item = Db.LineItems.Find(item.Id);
            item.Price = 1.111111111111m;
            Assert.IsTrue(Db.Entry(item).State == EntityState.Modified);
            Db.SaveChanges();

            item.Price = 1.1161111111m;
            Assert.IsTrue(Db.Entry(item).State == EntityState.Modified);
            Db.SaveChanges();
        }

        [TestMethod]
        public void attaching_entity_with_relations_and_saving_removes_relations()
        {
            var order = new Order(DateTime.Now)
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

            ReinitDb();
            order = Db.Order.Find(order.Id);
            Assert.AreEqual(order.Items.Count, 3);

            ReinitDb();
            Db.Configuration.ProxyCreationEnabled = false;
            order = SerializationUtils.DeepClone(Db.Order.Find(order.Id));
            order.OrderTime = DateTime.Now;
            Db.Entry(order).State = EntityState.Modified;
            Db.SaveChanges();
            Db.Configuration.ProxyCreationEnabled = true;

            ReinitDb();
            order = Db.Order.Find(order.Id);
            Assert.AreEqual(order.Items.Count, 0);

        }
    }
}
