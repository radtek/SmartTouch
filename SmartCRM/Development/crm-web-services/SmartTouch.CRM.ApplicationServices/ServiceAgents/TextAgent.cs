using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;

namespace SmartTouch.CRM.ApplicationServices.ServiceAgents
{
    public class TextAgent
    {
        public SendTextResponse SendText(SendTextRequest request)
        {
            TextService Text = new TextService();
            return Text.Send(request);
        }
    }
}
