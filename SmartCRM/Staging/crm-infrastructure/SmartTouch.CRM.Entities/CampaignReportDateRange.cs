using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum CampaignReportDateRange : byte
    {
        Last_7_Days = 1,
        Last_30_Days = 2,
        Last_3_Months = 3,
        Month_to_Date = 4,
        Year_to_Date=5,
        Last_Year = 6,
        Custom = 7
    }
}
