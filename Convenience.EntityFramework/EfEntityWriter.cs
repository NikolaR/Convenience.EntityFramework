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

        public EntityChangeOptions<T> ChangesFrom<T>(T entity)
        {
            AssertUtils.NotNull(entity, "entity");
            if (!Meta.IsEntity(entity.GetType()))
                throw new ArgumentException("Provided entity is not mapped in current database context");
            return new EntityChangeOptions<T>(this, entity);
        }

        public void ApplyEntityChanges(object entity)
        {
            AssertUtils.NotNull(entity, "entity");
            var dbEntity = Ctx.Set(entity.GetType()).Find(Meta.Properies.GetKey(entity));
            CopyDataProperties(entity, dbEntity);
        }

        public void ApplyGraphChanges<T1>(object entity)
        {
            ApplyGraphChanges(entity, typeof(T1));
        }

        public void ApplyGraphChanges<T1, T2>(object entity)
        {
            ApplyGraphChanges(entity, typeof(T1), typeof(T2));
        }

        public void ApplyGraphChanges<T1, T2, T3>(object entity)
        {
            ApplyGraphChanges(entity, typeof(T1), typeof(T2), typeof(T3));
        }

        public void ApplyGraphChanges<T1, T2, T3, T4>(object entity)
        {
            ApplyGraphChanges(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public void ApplyGraphChanges<T1, T2, T3, T4, T5>(object entity)
        {
            ApplyGraphChanges(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public void ApplyGraphChanges<T1, T2, T3, T4, T5, T6>(object entity)
        {
            ApplyGraphChanges(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public void ApplyGraphChanges<T>(T entity, params Type[] entityTypesToApply)
        {
            ApplyGraphChangesIntern(entity, new List<object>(), entityTypesToApply);
        }

        private void ApplyGraphChangesIntern(object entity, List<object> appliedEntities, params Type[] entityTypesToApply)
        {
            AssertUtils.NotNull(entity, "entity");
            if (appliedEntities.Contains(entity))
                return;
            if (entityTypesToApply == null)
                entityTypesToApply = new Type[0];
            Func<Type, bool> shouldApply = (t) => entityTypesToApply.Any(et => t.IsAssignableFrom(et));

            var key = Meta.Properies.GetKey(entity);
            var defaultKey = Meta.Properies.GetDefaultKey(entity);
            // Apply only for existing entities
            if (!CollectionUtils.ContentEqual(key, defaultKey))
                ApplyEntityChanges(entity);
            appliedEntities.Add(entity);
            var navProps = Meta.GetNavigationProperties(entity.GetType());
            foreach (var propertyInfo in navProps)
            {
                var value = propertyInfo.GetValue(entity);
                if (value != null && shouldApply(value.GetType()))
                    ApplyGraphChangesIntern(value, appliedEntities, entityTypesToApply);
                var collection = value as IEnumerable;
                if (collection != null)
                {
                    var enumerator = collection.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        value = enumerator.Current;
                        if (!shouldApply(value.GetType()))
                            break;
                        ApplyGraphChangesIntern(value, appliedEntities, entityTypesToApply);
                    }
                }
            }
        }

        internal void CopyDataProperties<T>(T source, T target) where T : class, new()
        {
            // Speed up copying routine
            // http://msmvps.com/blogs/jon_skeet/archive/2008/08/09/making-reflection-fly-and-exploring-delegates.aspx
            var props = Meta.GetDataProperties(target.GetType());
            foreach (var propertyInfo in props)
                propertyInfo.SetValue(target, propertyInfo.GetValue(source));
        }

        internal void ApplyUsingPaths<T>(EntityChangeOptions<T> entityChangeOptions)
        {
            List<object> appliedEntities = new List<object>();
            ApplyUsingPathsIntern(entityChangeOptions, appliedEntities);
        }

        internal void ApplyUsingPathsIntern<T>(EntityChangeOptions<T> entityChangeOptions, List<object> appliedEntities)
        {
            var entity = entityChangeOptions.Entity;
            if (appliedEntities.Contains(entity))
                return;

            var key = Meta.Properies.GetKey(entity);
            var defaultKey = Meta.Properies.GetDefaultKey(entity);
            // Apply only for existing entities
            if (!CollectionUtils.ContentEqual(key, defaultKey))
                ApplyEntityChanges(entity);
            ApplyEntityChanges(entity);
            appliedEntities.Add(entity);
            var navProps = Meta.GetNavigationProperties(entity.GetType());
            foreach (var propertyInfo in navProps)
            {
                var value = propertyInfo.GetValue(entity);
                EntityChangeOptions<T> subTree;
                var include = entityChangeOptions.ShouldIncludeNavigation(propertyInfo.Name, out subTree);
                if (!include)
                    continue;

                var collection = value as IEnumerable;
                if (collection != null)
                {
                    // Apply as collection
                    var enumerator = collection.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        subTree.Entity = enumerator.Current;
                        ApplyUsingPathsIntern(subTree, appliedEntities);
                    }
                }
                else
                {
                    // Apply as related entity
                    subTree.Entity = value;
                    ApplyUsingPathsIntern(subTree, appliedEntities);
                }
            }
        }
    }
}
