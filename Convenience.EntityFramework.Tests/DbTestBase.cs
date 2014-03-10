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
        private Lazy<EfMetaUtils> _efMetaUtils;
        private Lazy<EfEntityWriter> _efWriter;

        public void ReinitEfHelpers()
        {
            _efMetaUtils = new Lazy<EfMetaUtils>(() => new EfMetaUtils(Db));
            _efWriter = new Lazy<EfEntityWriter>(() => new EfEntityWriter(Db));
        }

        public void ReinitDb()
        {
            try
            {
                if (_db.IsValueCreated && _db.Value.Database.Exists())
                    _db.Value.Dispose();
            }
            catch { }

            _db = new Lazy<ShopContext>();
            ReinitEfHelpers();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            if (_db.IsValueCreated && _db.Value.Database.Exists())
                Db.Database.Delete();
            ReinitDb();
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
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

        public EfMetaUtils MetaUtils
        {
            get { return _efMetaUtils.Value; }
        }

        public EfEntityWriter EfWriter
        {
            get { return _efWriter.Value; }
        }
    }
}
