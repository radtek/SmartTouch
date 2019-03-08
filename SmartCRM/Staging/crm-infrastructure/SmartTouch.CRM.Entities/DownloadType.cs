using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum DownloadType : byte
    {
        [Description("CSV")]
        CSV = 1,
        [Description("Microsoft Excel")]
        Excel = 2,
        [Description("PDF")]
        PDF = 3
    }
}
