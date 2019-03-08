using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum EmailNotificationsCategory : byte
    {
        Default = 0,    //All non scheduled emails
        ActionReminders = 1,
        ActionSendEmail_Contacts = 2,
        TourReminders = 3,
        WorkflowSendEmail = 4,
        WorkflowFormSubmission = 5,
        WorkflowLeadAdapter = 6,
        WorkflowSavedSearch = 7,
        WorkflowCampaignLinkClick = 8,
        WorkflowWebVisit = 9,
        WorkflowTagAdd = 10,
        WorkflowTagRemove = 11,
        WorkflowLifecycleChange = 12,
        WorkflowLeadscoreReached = 13,
        WorkflowCampaignSent = 14,
        AccountRegistration = 15,
        AccountPause = 16,
        AccountClose = 17,
        InvalidCouponReport = 18,
        ContactSendEmail = 19,
        MailTesterEmail = 20,
        WebVisitInstantNotification = 21,
        WebVisitDailySummaryNotification = 22,
        FailedFormsReport = 23
    }
}
