using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum ContactSortFieldType : byte
    {
        NoSort = 0,
        RecentlyUpdatedContact = 1,
        FullName = 2,
        CompanyName = 3,
        MyContacts = 4,
        RecentlyViewed = 5,
        LeadScore=6,
        CampaignClickrate=7,
        LeadSource=8,
        FullNameOrCompanyName = 9
    }
}
