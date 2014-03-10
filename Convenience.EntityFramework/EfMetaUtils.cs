using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework
{
    public class EfMetaUtils
    {
        private Lazy<Type[]> _mappedEntityTypes;

        public EfMetaUtils(DbContext ctx)
        {
            AssertUtils.NotNull(ctx, "ctx");
            DbContext = ctx;

            _mappedEntityTypes = new Lazy<Type[]>(
                ()
                );
        }

        public DbContext DbContext
        { get; private set; }

        private Type[] GetEntityTypes()
        {
            DbContext.
        }
    }
}
