using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandMarkIT.Enterprise.CommunicationManager;
using LandMarkIT.Enterprise.CommunicationManager.Operations;
using LandMarkIT.Enterprise.CommunicationManager.Requests;
using LandMarkIT.Enterprise.CommunicationManager.Responses;
using AutoMapper;

namespace SmartTouch.CRM.Infrastructure.Agent
{
    public interface IAgent
    { }
    public class Agent : IAgent
    {
        public void TextRegistration()
        {

        }
        public void EmailRegistration()
        { }
        public SendMailResponse SendEmail(SendMailRequest request, Guid userToken)
        {
            Mail mail = new Mail(); return mail.Send(MailProvider.Smtp, userToken, request);
        }
        public SendTextResponse SendText(SendTextRequest request, Guid userToken)
        {
            Text Text = new Text(); return Text.Send(TextProvider.Twillio, userToken, request);
        }
    }
}
