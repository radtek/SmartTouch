using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum WorkflowTriggerType : byte
    {
        SmartSearch = 1,
        FormSubmitted = 2,
        LifecycleChanged = 3,
        TagApplied = 4,
        Campaign = 5,
        OpportunityStatusChanged = 6,
        LinkClicked = 7,
        TagRemoved = 8,
        LeadAdapterSubmitted = 9,
        LeadScoreReached = 10,
        WebPageVisited = 11,
        ActionCompleted = 12,
        TourCompleted = 13
    }
}
