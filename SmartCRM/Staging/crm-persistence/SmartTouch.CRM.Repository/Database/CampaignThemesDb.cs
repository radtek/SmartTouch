using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignThemesDb
    {
        [Key]
        public int CampaignThemeID { get; set; }

        public string Name { get; set; }
        public string ThemeType { get; set; }
        public ThemeStatus StatusID { get; set; }
        
        [ForeignKey("User1")]
        public int CreatedBy { get; set; }
        public UsersDb User1 { get; set; }

        [ForeignKey("User2")]
        public int? LastModifiedBy { get; set; }
        public UsersDb User2 { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }

        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public AccountsDb Account { get; set; }

        public bool IsDefault { get; set; }
        public string CSS { get; set; }
    }
}
