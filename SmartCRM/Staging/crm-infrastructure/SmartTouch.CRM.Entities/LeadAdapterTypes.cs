using System.ComponentModel;

namespace SmartTouch.CRM.Entities
{
    public enum LeadAdapterTypes : byte
    {
        [Description("BDX")]
        BDX = 1,

        [Description("NHG")]
        NHG = 2,

        [Description("Hot on Homes")]
        HotonHomes = 3,

        [Description("PROP Leads")]
        PROPLeads = 4,

        [Description("Zillow")]
        Zillow = 5,

        [Description("New Home Feed")]
        NewHomeFeed = 6,

        [Description("Private Communities")]
        PrivateCommunities = 7,

        [Description("IDX")]
        IDX = 8,

        [Description("Condo")]
        Condo = 9,

        [Description("Buzz Buzz Homes")]
        BuzzBuzzHomes = 10,
        [Description("Excel")]
        Import = 11,

        [Description("Home Finder")]
        HomeFinder = 12,

        [Description("Facebook Adapter")]
        Facebook = 13,

        [Description("Trulia")]
        Trulia = 14,

        [Description("Builder's Update")]
        BuildersUpdate = 15
    }
}
