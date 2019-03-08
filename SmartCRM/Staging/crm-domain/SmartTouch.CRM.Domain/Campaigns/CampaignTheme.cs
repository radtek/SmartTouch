using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignTheme : ValueObjectBase
    {
        public int CampaignThemeID { get; set; }
        public string Name { get; set; }
        public string ThemeType { get; set; }
        public ThemeStatus StatusID { get; set; }

        public int CreatedBy { get; set; }

        public int? LastModifiedBy { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }

        public int AccountID { get; set; }

        public bool IsDefault { get; set; }
        public string CSS { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
