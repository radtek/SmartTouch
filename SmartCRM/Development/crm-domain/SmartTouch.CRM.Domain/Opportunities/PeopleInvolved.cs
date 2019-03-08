using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Opportunities
{
    public class PeopleInvolved: ValueObjectBase
    {
        public int PeopleInvolvedID { get; set; }
        public int ContactID { get; set; }
        public short RelationshipTypeID { get; set; }
        public string ContactFullName { get; set; }
        public string RelationshipTypeName { get; set; }
        public string CompanyName { get; set; }
       
        public ContactType ContactType { get; set; }
       
        public short? LifeCycleStage { get; set; }

        protected override void Validate()
        {
        }
    }
}
