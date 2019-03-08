using MailChimp;
using MailChimp.Campaigns;
using MailChimp.Helper;
using MailChimp.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using SmartTouch.CRM.Domain.Contacts;
using HtmlAgilityPack;
using SmartTouch.CRM.Entities;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using LandmarkIT.Enterprise.CommunicationManager.MailTriggers;
using MailChimp.Reports;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    [CLSCompliant(false)]
    public class MailChimpCampaign : IMailProvider
    {
        MailChimpManager mailChimpManager = default(MailChimpManager);
        public MailChimpCampaign(string apiKey)
        {
            this.mailChimpManager = new MailChimpManager(apiKey);
        }

        public string SendCampaign(int campaignId, string campaignName, IEnumerable<EmailRecipient> emails, IEnumerable<Company> Companies, string title, string subject,
                                   string content, string fromEmail, string fromName, string campaignKey, int accountId, byte? campaignType, string listName = null)
        {
            content = content.Replace("*|CAMPID|*", campaignId.ToString()).Replace("<o:p>", "").Replace("</o:p>", "");//.FormatHTML();
            campaignName = Guid.NewGuid().ToString();
            var campaignCreateContent = campaignType == 132 ? new CampaignCreateContent { Text = content } : new CampaignCreateContent { HTML = content };
            var campaignCreateOptions = new CampaignCreateOptions
            {
                Title = title,
                FromEmail = fromEmail,
                FromName = fromName,
                Subject = subject,
            };

            var list = default(ListInfo);
            if (string.IsNullOrWhiteSpace(listName))
                list = mailChimpManager.GetLists().Data[0];
            else
                list = mailChimpManager.GetLists().Data.Where(li => string.Equals(li.Name, listName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            campaignCreateOptions.ListId = list.Id;
            campaignCreateOptions.AutoFooter = false;
            //subscribe every id to the list
            var batchEmailParameters = new List<BatchEmailParameter>();
            var emailParameters = new List<EmailParameter>();
            IEnumerable<string> mergervarlist = new List<string> { list.Id };
            MergeVarResult resultdata = mailChimpManager.GetMergeVars(mergervarlist);
            IEnumerable<MergeVarItemResult> mergevardata = resultdata.Data.Select(x => x.MergeVars).FirstOrDefault();

            // delete all mergervars
            foreach (var mergevar in mergevardata)
            {
                try
                {
                    mailChimpManager.DeleteMergeVar(list.Id, mergevar.Tag);
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("Exception" + ex);
                }
            }

            //add mergevars
            var pc = emails.FirstOrDefault();
            var fields = (pc.ContactFields != null) ? pc.ContactFields.Keys : new List<string>();
            var defaultValue = string.Empty;
            var displayName = string.Empty;
            foreach (var field in fields)
            {
                if (field.Contains("CF") || field.Contains("DF"))
                {
                    defaultValue = "XXXXX";
                    displayName = field;
                }
                else
                {
                    var propInfo = typeof(RecipientMergeVar).GetProperties().Where(p => p.Name == field).FirstOrDefault();
                    if (propInfo != null)
                        displayName = propInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().FirstOrDefault().DisplayName;
                }
                mailChimpManager.AddMergeVar(list.Id, field, displayName, new MergeVarOptions() { FieldType = "text", DefaultValue = defaultValue });
            }
            //set merge vars
            string replaceValue = string.Empty;
            foreach (var contact in emails)
            {
                var rmv = new RecipientMergeVar();

                if (contact.ContactFields != null && contact.ContactFields.Keys != null)
                {
                    var keys = contact.ContactFields.Keys;
                    var properties = rmv.GetType().GetProperties();
                    foreach (var key in keys)
                    {
                        replaceValue = contact.ContactFields[key].ToString();
                        if (key.Contains("CF") || key.Contains("DF"))
                        {
                            //TODO
                            /*
                             * Write code to handle muliti select custom field.
                             */
                            try
                            {
                                mailChimpManager.SetMergeVar(list.Id, key, replaceValue);
                            }
                            catch (MailChimp.Errors.MailChimpAPIException e)
                            {
                                ExceptionHandler.Current.HandleException(e, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Current.HandleException(e, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                            }
                        }
                        else
                        {
                            var pi = properties.Where(p => p.Name == key).FirstOrDefault();
                            if (pi != null)
                            pi.SetValue(rmv, replaceValue);

                        }
                    }
                }
                batchEmailParameters.Add(new BatchEmailParameter
                {
                    Email = new EmailParameter { Email = contact.EmailId },
                    MergeVars = rmv
                });

                emailParameters.Add(new EmailParameter { Email = contact.EmailId });
            }

            mailChimpManager.BatchSubscribe(list.Id, batchEmailParameters, doubleOptIn: false, updateExisting: true, replaceInterests: false);
            var segmentResult = mailChimpManager.AddStaticSegment(list.Id, campaignName);
            mailChimpManager.AddStaticSegmentMembers(list.Id, segmentResult.NewStaticSegmentID, emailParameters);
            var segmentOptions = new CampaignSegmentOptions();
            segmentOptions.Match = "All";
            segmentOptions.Conditions = new List<CampaignSegmentCriteria>();
            segmentOptions.Conditions.Add(new CampaignSegmentCriteria { Field = "static_segment", Operator = "eq", Value = segmentResult.NewStaticSegmentID.ToString() });

            string cId = "";

            try
            {
                Campaign result = mailChimpManager.CreateCampaign("regular", campaignCreateOptions, campaignCreateContent, segmentOptions, null);
                cId = result.Id;
                mailChimpManager.SendCampaign(cId);
            }
            catch (MailChimp.Errors.MailChimpAPIException e)
            {
                ExceptionHandler.Current.HandleException(e, DefaultExceptionPolicies.LOG_AND_RETHROW_POLICY);
            }

            return cId;
        }


        public string SendTransactionalEmail(string recipientEmail, string content, string title, string fromName, string fromEmail, string subject
            , string emailName, string listName = null)
        {
            var emailContent = new CampaignCreateContent { HTML = content };
            var emailOptions = new CampaignCreateOptions
            {
                Title = title,
                FromEmail = fromEmail,
                FromName = fromName,
                Subject = subject
            };
            var list = default(ListInfo);
            if (string.IsNullOrWhiteSpace(listName))
                list = mailChimpManager.GetLists().Data[0];
            else
                list = mailChimpManager.GetLists().Data.Where(li => string.Equals(li.Name, listName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            emailOptions.ListId = list.Id;
            emailOptions.AutoFooter = false;
            var batchEmailParameters = new List<BatchEmailParameter>();
            var emailParameters = new List<EmailParameter>();
            batchEmailParameters.Add(new BatchEmailParameter
            {
                Email = new EmailParameter { Email = recipientEmail },
            });
            emailParameters.Add(new EmailParameter { Email = recipientEmail });
            mailChimpManager.BatchSubscribe(list.Id, batchEmailParameters, doubleOptIn: false, updateExisting: true, replaceInterests: false);
            var segmentResult = mailChimpManager.AddStaticSegment(list.Id, emailName);
            mailChimpManager.AddStaticSegmentMembers(list.Id, segmentResult.NewStaticSegmentID, emailParameters);
            var segmentOptions = new CampaignSegmentOptions();
            segmentOptions.Match = "All";
            segmentOptions.Conditions = new List<CampaignSegmentCriteria>();
            segmentOptions.Conditions.Add(new CampaignSegmentCriteria { Field = "static_segment", Operator = "eq", Value = segmentResult.NewStaticSegmentID.ToString() });

            Campaign result = mailChimpManager.CreateCampaign("regular", emailOptions, emailContent, segmentOptions, null);
            string cId = result.Id;
            mailChimpManager.SendCampaign(cId);
            return cId;
        }


        public int BatchCount
        {
            get { return 0; }
        }

        public string SendCampaign(SmartTouch.CRM.Domain.Campaigns.Campaign campaign,
            List<EmailRecipient> emails,
            IEnumerable<SmartTouch.CRM.Domain.ValueObjects.FieldValueOption> customFieldsValueOptions,
            string accountCode, string accountAddress, string accountDomain, string providerEmail, Database.MailRegistrationDb mailRegistration)
        {
            return SendCampaign(campaign.Id, campaign.Name, emails, null, campaign.Subject, campaign.Subject, campaign.HTMLContent, campaign.From, campaign.SenderName, null, campaign.AccountID, campaign.CampaignTypeId, null);
        }

        public IEnumerable<string> AnalyzeCampaign(string serviceProviderCampaignId, out int total)
        {
            SentToMembers analytics = mailChimpManager.GetReportSentTo(serviceProviderCampaignId);
            total = analytics.Total;
            return analytics.Data.Select(c => c.Member.Email.ToString());
        }
    }
}
