using System.ComponentModel;

namespace SmartTouch.CRM.Entities
{
    public enum LeadAdapterRecordStatus : byte
    {       
        Added = 1,
        Duplicate = 2,
        Updated = 3,
        [Description("Validation Failed")]
        ValidationFailed = 4,
        Undefined = 5,
        BuilderNumberFailed = 6,
        DuplicateFromFile = 7,
        SystemFailure = 8,
        CommunityNumberFailed = 9
    }
}
