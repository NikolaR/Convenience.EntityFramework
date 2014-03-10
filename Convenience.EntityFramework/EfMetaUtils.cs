using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Convenience.EntityFramework
{
    public class EfMetaUtils
    {
        private readonly Lazy<ObjectItemCollection> _objectItemCollection;
        private readonly Lazy<EntityType[]> _entityTypes;
        private readonly Lazy<Type[]> _entityClrTypes;
        private readonly Lazy<EntityInfo[]> _entityInfos;
        private readonly Lazy<TwoWayDictionary<Type, EntityType>> _entityTypeLookupFac;
        private readonly EfPropertyUtils _propertyUtils;

        public EfMetaUtils(DbContext ctx)
        {
            AssertUtils.NotNull(ctx, "ctx");
            DbContext = ctx;

            _objectItemCollection = new Lazy<ObjectItemCollection>(GetObjectItemCollection);
            _entityTypes = new Lazy<EntityType[]>(GetEntityTypes);
            _entityClrTypes = new Lazy<Type[]>(GetEntityClrTypes);
            _entityInfos = new Lazy<EntityInfo[]>(GetEntityInfos);
            _entityTypeLookupFac = new Lazy<TwoWayDictionary<Type, EntityType>>(() => new TwoWayDictionary<Type, EntityType>(EntityClrTypes, EntityTypes));

            _propertyUtils = new EfPropertyUtils(this);
        }

        public DbContext DbContext
        { get; private set; }

        public ObjectItemCollection ObjectItemCollection
        {
            get { return _objectItemCollection.Value; }
        }

        public EntityType[] EntityTypes
        {
            get { return _entityTypes.Value; }
        }

        public Type[] EntityClrTypes
        {
            get { return _entityClrTypes.Value; }
        }

        public EntityInfo[] EntityInfos
        {
            get { return _entityInfos.Value; }
        }

        public TwoWayDictionary<Type, EntityType> EntityTypeLookup
        {
            get { return _entityTypeLookupFac.Value; }
        }

        public EfPropertyUtils Properies
        {
            get { return _propertyUtils; }
        }

        public ObjectItemCollection GetObjectItemCollection()
        {
            var objContext = ((IObjectContextAdapter)DbContext).ObjectContext;
            var workspace = objContext.MetadataWorkspace;
            var objectCollection = (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace);
            return objectCollection;
        }

        public EntityType[] GetEntityTypes()
        {
            return ObjectItemCollection.Where(i => i.BuiltInTypeKind == BuiltInTypeKind.EntityType).Cast<EntityType>().ToArray();
        }

        public Type[] GetEntityClrTypes()
        {
            var clrTypes = new Type[EntityTypes.Length];
            for (int i = 0; i < EntityTypes.Length; i++)
                clrTypes[i] = ObjectItemCollection.GetClrType(EntityTypes[i]);
            return clrTypes;
        }

        public EntityInfo[] GetEntityInfos()
        {
            return EntityTypes.Zip(EntityClrTypes, (et, t) => new EntityInfo(t, et)).ToArray();
        }

        public bool IsEntity(Type type)
        {
            type = UnwrapProxyType(type);
            return EntityTypeLookup.Any(et => et.First.IsAssignableFrom(type));
        }

        public PropertyInfo[] GetNavigationProperties(Type type)
        {
            type = UnwrapProxyType(type);
            if (!IsEntity(type))
                throw new ArgumentException("Provided type is not a mapped entity");

            return _propertyUtils.GetNavigationProperties(type);
        }

        public PropertyInfo[] GetDataProperties(Type type)
        {
            type = UnwrapProxyType(type);
            if (!IsEntity(type))
                throw new ArgumentException("Provided type is not a mapped entity");

            return _propertyUtils.GetDataProperties(type);
        }

        /// <summary>
        /// If provided type is an EntityFramework proxy, unwrap it and return actual entity type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal Type UnwrapProxyType(Type type)
        {
            while (type != null && !EntityTypeLookup.Contains(type))
                type = type.BaseType;
            return type;
        }
    }
}
