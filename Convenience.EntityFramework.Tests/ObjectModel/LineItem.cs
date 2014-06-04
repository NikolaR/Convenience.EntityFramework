using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework.Tests.ObjectModel
{
    [DebuggerDisplay("Item: {Name} (${Price})")]
    [Serializable]
    public class LineItem : Entity
    {
        public LineItem()
        { }

        public LineItem(string name, decimal price)
        {
            Name = name;
            Price = price;
        }

        public virtual string Name { get; set; }
        public virtual decimal Price { get; set; }
        public virtual Order Order { get; set; }
    }
}
