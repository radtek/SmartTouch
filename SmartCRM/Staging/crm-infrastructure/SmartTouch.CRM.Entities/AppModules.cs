using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum AppModules : byte
    {
        Accounts = 1,
        Users = 2,
        Contacts = 3,
        Campaigns = 4,
        ContactActions = 5,
        ContactNotes = 6,
        ContactTours = 7,
        ContactRelationships = 8,
        Tags = 9,
        Forms = 10,
        Opportunity = 16,
        OpportunityActions = 17,
        OpportunityNotes = 18,
        OpportunityTags = 32,
        LeadAdapter = 19,
        DropdownFields = 20,
        CustomFields = 21,
        LeadScore = 22,
        ImportData = 23,
        Reports = 24,
        SendMail = 25,
        SendText = 26,
        Roles = 27,
        AccountSettings = 29,
        ChangeOwner = 30,
        AdvancedSearch = 31,
        Automation = 33,
        Dashboard=34,
        WebAnalytics = 35,
        FullContact = 36,
        ImageDomains = 70,
        SeedList = 71,
        ApiKeys = 37,
        Download = 72,
        MarketingMessageCenter = 73,
        GlobalSenderList = 74,
        SuppressionList = 76,
        LitmusTest = 77,
        MailTester = 79,
        NeverBounce = 80,
        EmailValidator = 81
    }
}
