using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class EmailCampaign : EntityBase<int>, IAggregateRoot
    {
        public Campaign Campaign { get; set; }
       

        public byte EmailProviderID { get; set; }

        protected override void Validate()
        {
            IsCampaignValid();
            IsToMailingListValid();
        }

        public bool IsCampaignValid()
        {
            bool result =  Campaign.IsNameValid() && Campaign.IsContentValid() && Campaign.IsSubjectValid();
            return result;
        }

        public bool IsToMailingListValid()
        {
            bool result = To != null && To.Count == 0 ? false : true;
            return result;
        }

        public bool IsFromEmailValid()
        {
            bool result = From != null && From.IsValidEmail(From.EmailId);
            return result;
        }

        public bool IsScheduleTimeValid()
        {
            bool result = CampaignStatus == Entities.CampaignStatus.Scheduled && ScheduleTime >= DateTime.Now ? true : false;
            return result;
        }
    }
}
