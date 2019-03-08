using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
   public class ProviderRegistrationViewModel
    {
       public int RegistrationId { get; set; }
       public Guid RequestGuid { get; set; }
       public string ApiKey { get; set; }
       public string UserName { get; set; }
       public string LoginToken { get; set; }
       public string Password { get; set; }
       public string Host { get; set; }
       public bool IsDefault { get; set; }
       public string MailChimpListID { get; set; }
       public string Email { get; set; }
       public string SenderFriendlyName { get; set; }
       public CommunicationType CommunicationType { get; set; }
       public LandmarkIT.Enterprise.CommunicationManager.Requests.MailProvider MailProviderID { get; set; }
       public LandmarkIT.Enterprise.CommunicationManager.Requests.TextProvider TextProviderID { get; set; }
       public MailType MailProviderType { get; set; }
       public string VMTA { get; set; }
       public string SenderDomain { get; set; }
       public string ImageDomain { get; set; }
       public byte? ImageDomainId { get; set; }
       public int? Port { get; set; }
       public int ServiceProviderID { get; set; }
       public string SenderPhoneNumber { get; set; }
       public string ProviderName { get; set; }
       public IList<dynamic> Providers { get; set; }
    }
   public class ServiceProviderRegistrationViewModel
   {
       public IEnumerable<ProviderRegistrationViewModel> RegistrationViewModels { get; set; }
   
   }

}
