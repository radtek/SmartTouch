using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class SubscriptionSettingTypesDb
    {
        [Key]
        public int SubscriptionSettingTypeID { get; set; }
        public byte Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
