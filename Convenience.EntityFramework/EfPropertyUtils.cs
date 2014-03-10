using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework
{
    internal class EfPropertyUtils
    {
        private readonly EfMetaUtils _metaUtils;
        private CachingFactory<Type, PropertyInfo[]> _dataPropertiesFac;
        private CachingFactory<Type, PropertyInfo[]> _navigationPropertiesFac;
        private CachingFactory<Type, PropertyInfo[]> _writablePropertiesFac;

        public EfPropertyUtils(EfMetaUtils metaUtils)
        {
            _metaUtils = metaUtils;

            _navigationPropertiesFac = new CachingFactory<Type, PropertyInfo[]>(GetNavigationPropertiesIntern);
            _dataPropertiesFac = new CachingFactory<Type, PropertyInfo[]>(GetDataPropertiesIntern);
            _writablePropertiesFac = new CachingFactory<Type, PropertyInfo[]>(GetWritablePropertiesIntern);
        }

        public PropertyInfo[] GetWritableProperties(Type type)
        {
            return _writablePropertiesFac[type];
        }

        public PropertyInfo[] GetDataProperties(Type type)
        {
            return _dataPropertiesFac[type];
        }


        public PropertyInfo[] GetNavigationProperties(Type type)
        {
            return _navigationPropertiesFac[type];
        }

        internal bool IsNavigationProperty(Type entityType, PropertyInfo prop)
        {
            bool isNav;
            if (!_metaUtils.IsEntity(prop.ReflectedType))
                isNav = false;
            else
            {
                var et = _metaUtils.EntityTypeLookup[entityType];
                isNav = et.NavigationProperties.Any(navProp => navProp.Name == prop.Name);
            }
            return isNav;
        }

        private PropertyInfo[] GetDataPropertiesIntern(Type type)
        {
            var props = GetWritableProperties(type);
            return props.Where(p => !IsNavigationProperty(type, p)).ToArray();
        }

        private PropertyInfo[] GetNavigationPropertiesIntern(Type type)
        {
            var props = GetWritableProperties(type);
            return props.Where(p => IsNavigationProperty(type, p)).ToArray();
        }

        private PropertyInfo[] GetWritablePropertiesIntern(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.CanRead).ToArray();
        }
    }
}
