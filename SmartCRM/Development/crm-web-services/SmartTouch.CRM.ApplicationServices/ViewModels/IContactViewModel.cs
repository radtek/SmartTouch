using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    /// <summary>
    /// ContactViewModel for contact.
    /// </summary>
    public interface IContactViewModel {
        
        int ContactID { get; set; }
        string ContactImageUrl { get; set; }
        int AccountID { get; set; }
        int LeadScore { get; set; }
        string WorkPhone { get; set; }
        string PrimaryEmail { get; set; }
        bool DoNotEmail { get; set; }

        CommunicationViewModel Communication { get; set; }
        IList<AddressViewModel> Addresses { get; set; }
    }
}
