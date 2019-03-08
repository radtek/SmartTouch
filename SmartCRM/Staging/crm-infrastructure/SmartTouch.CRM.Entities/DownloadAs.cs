using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum DownloadAs
    {
        [Description("CSV")]
        CSV,
        [Description("Microsoft Excel")]
        Excel
    }
}
