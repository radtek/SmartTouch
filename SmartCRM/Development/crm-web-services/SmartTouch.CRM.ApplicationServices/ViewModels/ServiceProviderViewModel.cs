using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
   public interface IServiceProviderViewModel
    {
        int CommunicationLogID { get; set; }
        CommunicationType CommunicationTypeID { get; set; }
        System.Guid LoginToken { get; set; }
        int CreatedBy { get; set; }
        System.DateTime CreatedDate { get; set; }
        int AccountID { get; set; }
        string ProviderName { get; set; }
        MailType MailType { get; set; }
        string SenderPhoneNumber { get; set; }
    }
   public class ServiceProviderViewModel : IServiceProviderViewModel
   {
       public int CommunicationLogID { get; set; }
       public CommunicationType CommunicationTypeID { get; set; }
       public System.Guid LoginToken { get; set; }
       public int CreatedBy { get; set; }
       public System.DateTime CreatedDate { get; set; }
       public int AccountID { get; set; }
       public string ProviderName { get; set; }
       public MailType MailType { get; set; }
       public string SenderPhoneNumber { get; set; }
       public string AccountCode { get; set; }
       public byte? ImageDomainId { get; set; }
       public ImageDomainViewModel ImageDomain { get; set; }
       public virtual AccountViewModel Account { get; set; }
   }
}
