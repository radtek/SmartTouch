using SmartTouch.CRM.ApplicationServices.Messaging.MarketingMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IMarketingMessageService
    {
        InsertMarketingMessageResponse insertMarketingMessage(InsertMarketingMessageRequest request);
        UpdateMarketingMessageResponse updateMarketingMessage(UpdateMarketingMessageRequest request);
        GetMarketingMessageResponseById GetMarketingMessageById(GetMarketingMessageRequestById request);
        DeleteMarketingMessageResponse deleteMarketingMessage(DeleteMarketingMessageRequest request);
        GetAllMarketingMessagesResponse GetAllMarketingMessages(GetAllMarketingMessagesRequest request);
        GetAllMarketingMessageContentResponse GetAllMarketingMessageContents(GetAllMarketingMessageContentRequest request);
    }
}
