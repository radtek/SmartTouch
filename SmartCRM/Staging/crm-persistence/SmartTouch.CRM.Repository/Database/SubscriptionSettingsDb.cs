using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class SubscriptionSettingsDb
    {
        [Key]
        public int SubscriptionSettingsID { get; set; }
        public byte SubscriptionID { get; set; }
        public byte Name { get; set; }
        public string Value { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public string Description { get; set; }
    }
}
