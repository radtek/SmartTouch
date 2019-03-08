using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.MarketingMessageCenter
{
   public interface IMarketingMessagesRopository: IRepository<MarketingMessage, string>
    {
        IEnumerable<MarketingMessage> GetAllMessages(int limit, int pagenumber);
        void InsertMarketingAccounts(IEnumerable<MarketingMessageAccountMap> message);
        void InsertMarketingMessageContents(IEnumerable<MarketingMessageContentMap> message);
        void DeleteMarketingAccounts(int marketingMessageID);
        void DeleteMarketingMessageContents(int marketingMessageID);
        MarketingMessage GetMarketingMessageById(int marketingMessageID);
        void DeleteMarketingMessage(int[] Ids);
        IEnumerable<MarketingMessageContentMap> GetMarketingMessageConentsByAccount(int accountId);
        bool IsValidMarketingMessage(MarketingMessage message);
        int InsertMarketingMessage(MarketingMessage message);
        void UpdateMarketingMessage(MarketingMessage message);
        IEnumerable<string> GetAllPublishedAccounts(IList<int> accountIds, int messageId,DateTime? fromDate,DateTime? toDate);
    }
}
