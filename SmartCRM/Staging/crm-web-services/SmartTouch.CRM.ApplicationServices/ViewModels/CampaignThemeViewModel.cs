using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CampaignThemeViewModel
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
    }
}
