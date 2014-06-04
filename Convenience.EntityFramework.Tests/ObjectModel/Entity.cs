using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework.Tests.ObjectModel
{
    [Serializable]
    public class Entity
    {
        public long Id { get; set; }

        public DateTime LastUpdateTs { get; set; }
    }
}
