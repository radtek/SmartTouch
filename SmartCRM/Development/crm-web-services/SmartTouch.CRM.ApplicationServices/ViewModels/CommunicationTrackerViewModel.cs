
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ICommunicationTrackerViewModel
    {
        int CommunicationTrackerID { get; set; }
        bool? Address { get; set; }
        System.Guid TrackerGuid { get; set; }
        System.DateTime CreatedDate { get; set; }
        CommunicationStatus CommunicationStatusID { get; set; }
        CommunicationType CommunicationTypeID { get; set; }
        int? ContactID { get; set; }
    }

    public class CommunicationTrackerViewModel : ICommunicationTrackerViewModel
    {

        public bool? Address { get; set; }
        public System.Guid TrackerGuid { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public CommunicationStatus CommunicationStatusID { get; set; }
        public CommunicationType CommunicationTypeID { get; set; }
        public int? ContactID { get; set; }
        public int CommunicationTrackerID { get; set; }
    }
}
