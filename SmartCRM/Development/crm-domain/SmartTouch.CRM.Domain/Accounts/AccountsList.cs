using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Accounts
{
    public class AccountsList
    {
        public int TotalHits { get; set; }
        public IEnumerable<AccountsGridData> AccountGridData { get; set; }
    }

}
