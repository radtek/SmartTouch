using System.ComponentModel.DataAnnotations;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadAdapterErrorStatusDb
    {
        [Key]
        public LeadAdapterErrorStatus LeadAdapterErrorStatusID { get; set; }
        public string LeadAdapterErrorStatus { get; set; }
    }
}
