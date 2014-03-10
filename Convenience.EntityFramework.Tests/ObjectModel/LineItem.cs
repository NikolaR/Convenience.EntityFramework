using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework.Tests.ObjectModel
{
    [DebuggerDisplay("Item: {Name} (${Price})")]
    public class LineItem
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual decimal Price { get; set; }
    }
}
