using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework.Tests.ObjectModel
{
    [DebuggerDisplay("Customer: {Name}")]
    [Serializable]
    public class Customer
    {
        public Customer()
        { }
        public Customer(string name)
        {
            Name = name;
        }

        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual List<Order> Orders { get; set; }
    }
}
