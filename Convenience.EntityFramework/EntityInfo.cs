using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework
{
    public class EntityInfo
    {
        internal EntityInfo(Type type, EntityType entityType)
        {
            this.Type = type;
            EntityType = entityType;
        }

        public EntityType EntityType
        { get; private set; }

        public Type Type
        { get; private set; }
    }
}
