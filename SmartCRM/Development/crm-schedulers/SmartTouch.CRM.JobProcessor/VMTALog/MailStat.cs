using CsvHelper;
using CsvHelper.Configuration;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor.VMTALog
{

    public enum BounceCat
    {
        BadDomain,
        InactiveMailbox,
        BadMailbox,
        Complained,
        BadConfiguration,
        BadConnection,
        ContentRelated,
        InvalidSender,
        MessageExpired,
        NoAnswerFromHost,
        PolicyRelated,
        ProtocolErrors,
        QuotaIssues,
        RelayingIssues,
        RoutingErrors,
        SpamRelated,
        VirusRelated,
        None,
        Other
    }

    public class MailStat
    {
        public CampaignDeliveryStatus DeliveryStatus { get; set; }
        public string Recipient { get; set; }
        public string Originator { get; set; }
        public int CampaignRecipientId { get; set; }
        public int CampaignId { get; set; }
        public short? OptOutStatus
        {
            get
            {
                short? status = null;
                if (BounceCategory == BounceCat.Complained)
                {
                    status = (short)EmailStatus.Complained;
                }

                return status;
            }
        }
        public BounceCat BounceCategory { get; set; }
        public string Remarks { get; set; }
        public DateTime TimeLogged { get; set; }
    }

    public class FeedBackLoopMailStat
    {
        public CampaignDeliveryStatus? DeliveryStatus { get; set; }
        public string Recipient { get; set; }
        public string Originator { get; set; }
        public int CampaignRecipientId { get; set; }
        public string Remarks { get; set; }
        public DateTime TimeLogged { get; set; }
    }

    public class MailMapping : CsvClassMap<MailStat>
    {
        public MailMapping()
        {
            Map(m => m.Recipient).Name("rcpt");
            Map(m => m.Originator).Name("orig");
            Map(m => m.Remarks).Name("dsnDiag");
            Map(m => m.TimeLogged).ConvertUsing(c => GetTimeLogged(c));
            Map(m => m.CampaignId).ConvertUsing(c => GetCampaignId(c));
            Map(m => m.BounceCategory).ConvertUsing(b => GetBounceCategory(b));
            Map(m => m.DeliveryStatus).ConvertUsing(s =>
            {
                var value = s.GetField<string>("type");
                var bounceCat = GetBounceCategory(s);
                List<BounceCat> hardBounceCategories = GetHardBounceCategories();
                List<BounceCat> softBounseCategories = GetSoftbounceCategories();
                List<BounceCat> blockedCategories = GetBlockedCategories();

                CampaignDeliveryStatus status = CampaignDeliveryStatus.Delivered;
                switch (value)
                {
                    case "b":
                    case "rb":
                        if (hardBounceCategories.Any(h=> h== bounceCat))
                            status = CampaignDeliveryStatus.HardBounce;
                        else if (softBounseCategories.Any(sb=> sb == bounceCat))
                            status = CampaignDeliveryStatus.SoftBounce;
                        else
                            status = CampaignDeliveryStatus.Blocked;
                        break;
                    case "d": status = CampaignDeliveryStatus.Delivered;
                        break;
                    default :
                        break;
                }
                return status;
            });


        }

        private DateTime GetTimeLogged(ICsvReaderRow c)
        {
            DateTime field = DateTime.UtcNow;
            try
            {
                field = c.GetField<DateTime>("timeLogged");
            }
            catch {  }
            return field;
        }

        private List<BounceCat> GetHardBounceCategories()
        {
            List<BounceCat> hardBounceCategories = new List<BounceCat>()
                {
                    BounceCat.BadDomain,
                    BounceCat.InactiveMailbox,
                    BounceCat.PolicyRelated
                    
                };
            return hardBounceCategories;
        }

        private List<BounceCat> GetSoftbounceCategories()
        {
            List<BounceCat> softBounseCategories = new List<BounceCat>()
            {
                BounceCat.NoAnswerFromHost,
                BounceCat.Other,
                BounceCat.QuotaIssues,
                BounceCat.RelayingIssues,
                BounceCat.RoutingErrors,
                BounceCat.PolicyRelated
            };
            return softBounseCategories;
        }

        private List<BounceCat> GetBlockedCategories()
        {
            List<BounceCat> blockedCategories = new List<BounceCat>()
            {
                BounceCat.BadConfiguration,
                BounceCat.ContentRelated,
                BounceCat.InvalidSender,
                BounceCat.BadConnection,
                BounceCat.MessageExpired,
                BounceCat.ProtocolErrors,
                BounceCat.SpamRelated,
                BounceCat.VirusRelated
            };
            return blockedCategories;
        }
        /// <summary>
        /// Get campaign id from header/jobid
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private int GetCampaignId(ICsvReaderRow c)
        {        
            string value = string.IsNullOrEmpty(c.GetField<string>("jobID")) ? string.Empty : c.GetField<string>("jobID");
            int campaignId = 0;
            if (value.Contains('/'))
            {
                try
                {
                    var length = value.Split('/').Length;
                    campaignId = Convert.ToInt32(value.Split('/')[length - 1]);
                }
                catch
                {
                    campaignId = 0;
                }
            }
            return campaignId;

        }
        /// <summary>
        /// Get bounce category
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private BounceCat GetBounceCategory(ICsvReaderRow b)
        {
            var value = b.GetField<string>("bounceCat");
            BounceCat bc = BounceCat.Other;
            switch (value)
            {
                case "other": bc = BounceCat.Other;
                    break;
                case "bad-mailbox": bc = BounceCat.BadMailbox;
                    break;
                case "bad-domain": bc = BounceCat.BadDomain;
                    break;
                case "inactive-mailbox": bc = BounceCat.InactiveMailbox;
                    break;
                case "bad-configuration": bc = BounceCat.BadConfiguration;
                    break;
                case "bad-connection": bc = BounceCat.BadConnection;
                    break;
                case "content-related": bc = BounceCat.ContentRelated;
                    break;
                case "invalid-sender": bc = BounceCat.InvalidSender;
                    break;
                case "message-expired": bc = BounceCat.MessageExpired;
                    break;
                case "no-answer-from-host": bc = BounceCat.NoAnswerFromHost;
                    break;
                case "policy-related": bc = BounceCat.PolicyRelated;
                    break;
                case "protocol-errors": bc = BounceCat.ProtocolErrors;
                    break;
                case "quota-issues": bc = BounceCat.QuotaIssues;
                    break;
                case "relaying-issues": bc = BounceCat.RelayingIssues;
                    break;
                case "routing-errors": bc = BounceCat.RoutingErrors;
                    break;
                case "spam-related": bc = BounceCat.SpamRelated;
                    break;
                case "virus-related": bc = BounceCat.VirusRelated;
                    break;
                default:
                    break;
            }
            return bc;
        }
    }

    public class FeedBackLoopMailMapping : CsvClassMap<FeedBackLoopMailStat>
    {
        public FeedBackLoopMailMapping()
        {
            Map(m => m.Recipient).Name("rcpt");
            Map(m => m.Originator).Name("orig");
            Map(m => m.Remarks).Name("dsnDiag");
            Map(m => m.TimeLogged).ConvertUsing(c => GetTimeLogged(c));
            Map(m => m.CampaignRecipientId).ConvertUsing(c => GetFeedbackCampaignRecipientId(c));
            Map(m => m.DeliveryStatus).ConvertUsing(s =>
            {
                CampaignDeliveryStatus? DS = null;
                var value = s.GetField<string>("feedbackType");
                if (value == "abuse")
                    DS = CampaignDeliveryStatus.Abuse;
                return DS;
            });
        }

        private DateTime GetTimeLogged(ICsvReaderRow c)
        {
            DateTime field = DateTime.UtcNow;
            try
            {
                field = c.GetField<DateTime>("timeLogged");
            }
            catch { }
            return field;
        }

        /// <summary>
        /// Get campaign id from header/jobid
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private int GetFeedbackCampaignRecipientId(ICsvReaderRow c)
        {
            string value = string.IsNullOrEmpty(c.GetField<string>("header_X-STCustomer")) ? string.Empty : c.GetField<string>("header_X-STCustomer");
            int campaignRecipientId = 0;
            int.TryParse(value, out campaignRecipientId);
            return campaignRecipientId;
        }

    }

}
