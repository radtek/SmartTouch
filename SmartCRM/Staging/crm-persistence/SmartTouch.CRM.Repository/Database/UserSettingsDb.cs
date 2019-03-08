using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class UserSettingsDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserSettingID { get; set; }
         [ForeignKey("Accounts")]
        public int AccountID { get; set; }
        [ForeignKey("Users")]
        public int UserID { get; set; }        
        [ForeignKey("Currency")]
        public byte? CurrencyID { get; set; }
        [ForeignKey("Country")]
        public string CountryID { get; set; }
        public string TimeZone { get; set; }
        public short ItemsPerPage { get; set; }
        public string EmailID { get; set; }
        public bool? DailySummary { get; set; }
        public bool? AlertNotification { get; set; }
        public bool? LeadScoreNotification { get; set; }
        public int? LeadScoreValue { get; set; }
        public bool? EmailNotification { get; set; }
        public bool? TextNotification { get; set; }
        public byte? DateFormat { get; set; }
        [NotMapped]
        public string DateFormatName { get; set; }
        public bool HasAcceptedTC { get; set; }
        public bool IsIncludeSignature { get; set; }
        public virtual AccountsDb Accounts { get; set; }
        public virtual UsersDb Users { get; set; }
        public virtual CurrenciesDb Currency { get; set; }
        public virtual CountriesDb Country { get; set; }
    }
}
 