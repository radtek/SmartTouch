using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class SaveRelationshipRequest : ServiceRequestBase
    {
        public int UserId { get; set; }
        public RelationshipViewModel RelationshipViewModel { get; set; }
        public string SelectAllSearchCriteria { get; set; }
        public string AdvancedSearchCritieria { get; set; }
        public int[] DrillDownContactIds { get; set; }       
    }
    public class SaveRelationshipResponse : ServiceResponseBase
    {
        public RelationshipViewModel RelationshipViewModel { get; set; }
    }
}
