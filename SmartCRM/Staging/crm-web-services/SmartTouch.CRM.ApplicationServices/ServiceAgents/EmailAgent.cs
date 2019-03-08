using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;

namespace SmartTouch.CRM.ApplicationServices.ServiceAgents
{
    public class EmailAgent
    {
        public SendMailResponse SendEmail(SendMailRequest request)
        {
            MailService mail = new MailService();
            return mail.Send(request);
        }
    }
}
