using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework
{
    public class EfEntityWriter
    {
        public EfEntityWriter(DbContext ctx)
        {
            AssertUtils.NotNull(ctx, "context");
            Ctx = ctx;
            Meta = new EfMetaUtils(ctx);
        }

        public DbContext Ctx
        { get; private set; }

        public EfMetaUtils Meta
        {
            get; private set;
        }

        public void CopyDataProperties<T>(T source, T target) where T : class, new()
        {
            var props = Meta.GetDataProperties(target.GetType());
            foreach (var propertyInfo in props)
                propertyInfo.SetValue(target, propertyInfo.GetValue(source));
        }
    }
}
