using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class PeopleInvolvedViewModel
    {
        public int OpportunityRelationMapID { get; set; }
        public int ContactID { get; set; }
        public short RelationshipTypeID { get; set; }
        public string ContactFullName { get; set; }
        public string CompanyName { get; set; }
        //public bool IsPrivate { get; set; }
        public ContactType ContactType { get; set; }
        public string RelationShipTypeName { get; set; }
        public short? LifeCycleStage { get; set; }
        public bool IsInEditMode { get; set; }
        public IList<UserViewModel> Users { get; set; }
        public IList<RelationshipTypesViewModel> Relationships { get; set; }
    }
}
