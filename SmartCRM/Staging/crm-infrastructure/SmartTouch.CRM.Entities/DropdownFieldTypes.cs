using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum DropdownFieldTypes : byte
    {
        PhoneNumberType = 1,
        AddressType = 2,
        LifeCycle = 3,
        PartnerType = 4,
        LeadSources = 5,
        OpportunityStage = 6,
        Community = 7,
        TourType = 8,
        RelationshipType = 9,
        ActionType = 10,
        NoteCategory = 11
    }
}
