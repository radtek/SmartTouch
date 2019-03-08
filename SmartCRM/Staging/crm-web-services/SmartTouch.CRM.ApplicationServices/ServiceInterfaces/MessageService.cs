using SmartTouch.CRM.ApplicationServices.Messaging.Messages;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public class MessageService :IMessageService
    {
        readonly IMessageRepository messageRepository;
        public MessageService(IMessageRepository messageRepository)
        {
            this.messageRepository = messageRepository;
        }
        /// <summary>
        /// Send messages related to automation/leadscore
        /// </summary>
        /// <param name="request"></param>
        public void SendMessages(SendMessagesRequest request)
        {
            var messages = new List<TrackMessage>();
            if (request.Message != null)
            {
                messages.Add(request.Message);
            }
            else
                messages = request.Messages.ToList();
            if (messages.Any())
                messageRepository.SendMessages(messages);
        }

        /// <summary>
        /// Gets leadscore messages
        /// </summary>
        /// <returns></returns>
        public GetLeadScoreMessagesResponse GetLeadScoreMessages()
        {
            return new GetLeadScoreMessagesResponse
            {
                Messages = messageRepository.GetLeadScoreMessages()
            };
        }
        /// <summary>
        /// Update lead score message
        /// </summary>
        /// <param name="request"></param>
        public void UpdateLeadScoreMessage(UpdateLeadScoreMessage request)
        {
            messageRepository.UpdateLeadScoreMessage(request.Messages);
        }
    }
}
