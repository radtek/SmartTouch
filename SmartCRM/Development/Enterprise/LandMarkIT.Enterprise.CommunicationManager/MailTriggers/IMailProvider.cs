using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Collections.Generic;

namespace LandmarkIT.Enterprise.CommunicationManager.MailTriggers
{
    public interface IMailProvider
    {
        int BatchCount { get; }
        string SendCampaign(Campaign campaign, List<EmailRecipient> emails, IEnumerable<FieldValueOption> customFieldsValueOptions
            , string accountCode, string accountAddress, string accountDomain
            , string providerEmail, MailRegistrationDb mailRegistration);
    }
}
