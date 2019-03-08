using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum Reports : byte
    {
        HotList = 1,
        CampaignList = 2,
        NewLeads = 3,
        OpportunityPipeline = 4,
        TrafficByLifecycle = 5,
        Activity = 6,
        TrafficByType = 7,
        TrafficBySource = 8,
        TrafficByTypeAndLifecycle = 9,
        FormsCountSummary = 10,
        BDXFreemiumCustomLeadReport = 11,
        TagsReport=12,
        BDXCustomLeadReport = 13,
        Custom =14,
        WebVisits = 15,
        FirstLeadSourceReport = 16,
        AllLeadSourceReport = 17,
        CampaignReEngagementReport = 18,
        DatabaseLifeCycleReport = 19,
        ToursByContactsReport = 20,
        NightlyCampaign = 21,
        NightlyStatus = 22,
        LoginFrequency = 23,
        BouncedEmail = 24
    }
}
