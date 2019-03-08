using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;

namespace LandmarkIT.Enterprise.CommunicationManager.Contracts
{
    public interface ITextService
    {
        SendTextResponse Send(SendTextRequest request);        
    }
}
