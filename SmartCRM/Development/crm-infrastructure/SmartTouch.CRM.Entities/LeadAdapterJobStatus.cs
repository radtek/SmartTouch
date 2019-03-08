
namespace SmartTouch.CRM.Entities
{
    public enum LeadAdapterJobStatus : byte
    {
        Undefined = 0,
        Passed = 1,
        Failed = 2,
        Completed = 3,
        Inprogress = 4,
        SQLCompleted = 5,
        Picked = 6,
        ReadyToProcess = 7,
        ProcessingContacts = 8
    }
}
