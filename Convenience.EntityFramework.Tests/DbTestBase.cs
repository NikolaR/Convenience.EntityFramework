using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Convenience.EntityFramework.Tests.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Convenience.EntityFramework.Tests
{
    public class DbTestBase
    {
        private Lazy<ShopContext> _db = new Lazy<ShopContext>();

        [TestInitialize]
        public void TestInitialize()
        {
            if (_db.IsValueCreated && _db.Value.Database.Exists())
                Db.Database.Delete();
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            if (_db.IsValueCreated)
                Db.Database.Delete();

            try
            {
                _db.Value.Database.Delete();
            }
            catch (Exception ex)
            {
                Assert.Inconclusive("Could not delete database, it may need to be removed manually.\n Check connection string for database location detail.\n Error Message:{0}", ex.Message);
            }
            finally
            {
                if (_db != null)
                    _db.Value.Dispose();
            }
        }

        public ShopContext Db
        {
            get
            {
                return _db.Value;
            }
        }
    }
}
