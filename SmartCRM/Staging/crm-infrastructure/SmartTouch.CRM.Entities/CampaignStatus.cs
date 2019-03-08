using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum CampaignStatus : short
    {
        Draft = 101,
        Scheduled = 102,
        Cancelled = 103,
        Sending = 104,
        Sent = 105,
        Queued = 106,
        Active = 107,
        Analyzing = 108,
        Archive = 109,
        Failure = 110,
        Delayed = 117,
        Retrying = 118
    }

    public enum CampaignContactActivity : byte
    {
        Open = 1,
        Click = 2
    }

    public enum CampaignDeliveryStatus : short
    {
        Delivered = 111,
        SoftBounce = 112,
        HardBounce = 113,
        Sent = 114,
        Abuse = 115,
        Failed = 116,
        Blocked = 119
    }

    public enum CampaignOptOutStatus : short
    {
        Unsubscribed = 54,
        Abuse = 56
    }
    public enum CampaignActivity : short
    {
        Opened = 1,
        Clicked = 2,
        Delivered = 3,
        Unsubscribed = 4,
        Complained = 5
    }

    public enum CampaignDrillDownActivity : short
    {
        Opened = 1,
        Clicked = 2,
        Delivered = 3,
        Unsubscribed = 4,
        Complained = 5,
        Sent = 6,
        Bounced = 7,
        NotViewed = 8,
        LinkClicked = 9,
        All = 10,
        Blocked = 11
    }

    public enum ThemeStatus : short
    {
        Inactive = 0,
        Active = 1
    }

    public enum LitmusCheckStatus : int
    {
        ReadyToProcesLitmusCheck = 131,
        LitmusCheckCompleted = 132,
        LitmusNotified = 133,
        LitmusCheckFailed = 134
    }
}
