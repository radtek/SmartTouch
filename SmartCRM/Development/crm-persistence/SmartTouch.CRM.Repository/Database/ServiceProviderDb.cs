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
   public class ServiceProvidersDb
    {
       [Key]
       public int ServiceProviderID { get; set; }
        public CommunicationType CommunicationTypeID { get; set; }
        public System.Guid LoginToken { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
         [ForeignKey("Accounts")]
        public int AccountID { get; set; }
        public string ProviderName { get; set; }
        public virtual AccountsDb Accounts { get; set; }
        public bool IsDefault { get; set; }
        public byte EmailType { get; set; }
        public string SenderPhoneNumber { get; set; }
    }
}
