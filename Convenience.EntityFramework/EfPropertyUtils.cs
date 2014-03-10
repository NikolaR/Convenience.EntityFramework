using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework
{
    public class EfPropertyUtils
    {
        private readonly EfMetaUtils _metaUtils;
        private readonly CachingFactory<Type, PropertyInfo[]> _dataPropertiesFac;
        private readonly CachingFactory<Type, PropertyInfo[]> _navigationPropertiesFac;
        private readonly CachingFactory<Type, PropertyInfo[]> _keyPropertiesFac;
        private readonly CachingFactory<Type, PropertyInfo[]> _writablePropertiesFac;

        internal EfPropertyUtils(EfMetaUtils metaUtils)
        {
            _metaUtils = metaUtils;

            _navigationPropertiesFac = new CachingFactory<Type, PropertyInfo[]>(GetNavigationPropertiesIntern);
            _dataPropertiesFac = new CachingFactory<Type, PropertyInfo[]>(GetDataPropertiesIntern);
            _writablePropertiesFac = new CachingFactory<Type, PropertyInfo[]>(GetWritablePropertiesIntern);
            _keyPropertiesFac = new CachingFactory<Type, PropertyInfo[]>(GetKeyPropertiesIntern);
        }

        public PropertyInfo[] GetWritableProperties(Type type)
        {
            if (!_metaUtils.IsEntity(type))
                throw new ArgumentException("Provided type is not an entity type");
            type = _metaUtils.UnwrapProxyType(type);
            return _writablePropertiesFac[type];
        }

        public PropertyInfo[] GetDataProperties(Type type)
        {
            if (!_metaUtils.IsEntity(type))
                throw new ArgumentException("Provided type is not an entity type");
            type = _metaUtils.UnwrapProxyType(type);
            return _dataPropertiesFac[type];
        }

        public PropertyInfo[] GetNavigationProperties(Type type)
        {
            if (!_metaUtils.IsEntity(type))
                throw new ArgumentException("Provided type is not an entity type");
            type = _metaUtils.UnwrapProxyType(type);
            return _navigationPropertiesFac[type];
        }

        public PropertyInfo[] GetKeyProperties(Type type)
        {
            if (!_metaUtils.IsEntity(type))
                throw new ArgumentException("Provided type is not an entity type");
            type = _metaUtils.UnwrapProxyType(type);
            return _keyPropertiesFac[type];
        }

        public object[] GetKey(object obj)
        {
            AssertUtils.NotNull(obj, "obj");
            var type = obj.GetType();
            if (!_metaUtils.IsEntity(type))
                throw new ArgumentException("Provided type is not an entity type");
            var keyProps = GetKeyProperties(type);
            var key = new object[keyProps.Length];
            for (int i = 0; i < keyProps.Length; i++)
            {
                var keyProp = keyProps[i];
                key[i] = keyProp.GetGetMethod().Invoke(obj, null);
            }
            return key;
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

        private PropertyInfo[] GetKeyPropertiesIntern(Type type)
        {
            var props = GetWritableProperties(type);
            var keyPropNames = _metaUtils.EntityTypeLookup[type].KeyProperties.Select(kp => kp.Name);
            return props.Where(p => keyPropNames.Contains(p.Name)).ToArray();
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
