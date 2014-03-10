﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework.Tests.ObjectModel
{
    [DebuggerDisplay("Order for {Items.Count} on {OrderTime}")]
    public class Order
    {
        public virtual long Id { get; set; }
        public virtual DateTime OrderTime { get; set; }
        public virtual List<LineItem> Items { get; set; } 
    }
}