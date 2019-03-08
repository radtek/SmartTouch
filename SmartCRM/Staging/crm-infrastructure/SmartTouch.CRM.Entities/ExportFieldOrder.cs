using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum ExportFieldOrder
    {
        [Description("A-Z")]
        Ascending = 1,
        [Description("Z-A")]
        Descending = 2       
    }
}
