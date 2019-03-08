
namespace LandmarkIT.Enterprise.CommunicationManager.Responses
{
    public enum CommunicationStatus : byte
    {
        Success = 1,
        Failed = 2,
        Rejected = 3,   //Receipients not found
        Queued = 4
    }
}
