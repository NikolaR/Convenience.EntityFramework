using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework
{
    /// <summary>
    /// Class for applying data changes from detached objects to database. For example,
    /// entity received from 
    /// </summary>
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
            get;
            private set;
        }

        public void ApplyDbEntityChanges(object entity)
        {
            AssertUtils.NotNull(entity, "entity");
            var dbEntity = Ctx.Set(entity.GetType()).Find(Meta.Properies.GetKey(entity));
            CopyDataProperties(entity, dbEntity);
        }

        public void ApplyDbGraphChanges<T1>(object entity)
        {
            ApplyDbGraphChanges(entity, typeof(T1));
        }

        public void ApplyDbGraphChanges<T1, T2>(object entity)
        {
            ApplyDbGraphChanges(entity, typeof(T1), typeof(T2));
        }

        public void ApplyDbGraphChanges<T1, T2, T3>(object entity)
        {
            ApplyDbGraphChanges(entity, typeof(T1), typeof(T2), typeof(T3));
        }

        public void ApplyDbGraphChanges<T1, T2, T3, T4>(object entity)
        {
            ApplyDbGraphChanges(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public void ApplyDbGraphChanges<T1, T2, T3, T4, T5>(object entity)
        {
            ApplyDbGraphChanges(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public void ApplyDbGraphChanges<T1, T2, T3, T4, T5, T6>(object entity)
        {
            ApplyDbGraphChanges(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public void ApplyDbGraphChanges<T>(T entity, params Type[] entityTypesToApply)
        {
            ApplyDbGraphChangesIntern(entity, new List<object>(), entityTypesToApply);
        }

        private void ApplyDbGraphChangesIntern(object entity, List<object> appliedEntities, params Type[] entityTypesToApply)
        {
            AssertUtils.NotNull(entity, "entity");
            if (appliedEntities.Contains(entity))
                return;
            if (entityTypesToApply == null)
                entityTypesToApply = new Type[0];
            Func<Type, bool> shouldApply = (t) => entityTypesToApply.Any(et => t.IsAssignableFrom(et));

            ApplyDbEntityChanges(entity);
            appliedEntities.Add(entity);
            var navProps = Meta.GetNavigationProperties(entity.GetType());
            foreach (var propertyInfo in navProps)
            {
                var value = propertyInfo.GetValue(entity);
                if (value != null && shouldApply(value.GetType()))
                    ApplyDbGraphChanges(value);
                var collection = value as IEnumerable;
                if (collection != null)
                {
                    var enumerator = collection.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        value = enumerator.Current;
                        if (!shouldApply(value.GetType()))
                            break;
                        ApplyDbGraphChangesIntern(value, appliedEntities, entityTypesToApply);
                    }
                }
            }
        }

        internal void CopyDataProperties<T>(T source, T target) where T : class, new()
        {
            var props = Meta.GetDataProperties(target.GetType());
            foreach (var propertyInfo in props)
                propertyInfo.SetValue(target, propertyInfo.GetValue(source));
        }
    }
}
