using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum OutlookSyncStatus : short
    {
        NotInSync = 11,
        InSync = 12,
        Syncing = 13,
        Deleted = 14,
        NewInCRM = 15,
        UpdatedInCRM = 16,
        DeletedInCRM = 17,
        UpdatedInOutlook = 18,
        DeletedInOutlook = 19
    }
}
