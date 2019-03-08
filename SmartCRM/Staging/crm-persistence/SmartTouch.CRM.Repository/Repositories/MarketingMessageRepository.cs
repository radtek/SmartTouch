using AutoMapper;
using SmartTouch.CRM.Domain.MarketingMessageCenter;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;
using System.Data.Entity;
using LinqKit;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Repositories
{
   public class MarketingMessageRepository:  Repository<MarketingMessage,int, MarketingMessagesDb>,IMarketingMessagesRopository
    {
        public MarketingMessageRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        public override void PersistValueObjects(MarketingMessage domainType, MarketingMessagesDb dbType, CRMDb db)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserting  Marketing Messages
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public int InsertMarketingMessage(MarketingMessage message)
        {
            var db = ObjectContextFactory.Create();
            MarketingMessagesDb marketingMessage = Mapper.Map<MarketingMessage, MarketingMessagesDb>(message);
            db.MarketingMessages.Add(marketingMessage);
            db.SaveChanges();
            int messageID = marketingMessage.MarketingMessageID;
            return messageID;
        }

        /// <summary>
        /// Updating Marketing Messages
        /// </summary>
        /// <param name="message"></param>
        public void UpdateMarketingMessage(MarketingMessage message)
        {
            var db = ObjectContextFactory.Create();
            MarketingMessagesDb marketingMessage = Mapper.Map<MarketingMessage, MarketingMessagesDb>(message);
            db.Entry(marketingMessage).State = EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// Inserting  Marketing Accounts
        /// </summary>
        /// <param name="domainType"></param>
        public void InsertMarketingAccounts(IEnumerable<MarketingMessageAccountMap> domainType)
        {
            var db = ObjectContextFactory.Create();
            List<MarketingMessageAccountMapDb> accountsdb = new List<MarketingMessageAccountMapDb>();

            foreach(MarketingMessageAccountMap account in domainType)
            {
                accountsdb.Add(Mapper.Map<MarketingMessageAccountMap, MarketingMessageAccountMapDb>(account));
            }
            db.MarketingMessageAccountMap.AddRange(accountsdb);
            db.SaveChanges();
          
        }
        /// <summary>
        /// Inserting  Marketing Contents
        /// </summary>
        /// <param name="domainType"></param>
        public void InsertMarketingMessageContents(IEnumerable<MarketingMessageContentMap> domainType)
        {
            var db = ObjectContextFactory.Create();
            List<MarketingMessageContentMapDb> messagesdb = new List<MarketingMessageContentMapDb>();

            foreach (MarketingMessageContentMap account in domainType)
            {
                messagesdb.Add(Mapper.Map<MarketingMessageContentMap, MarketingMessageContentMapDb>(account));
            }
            db.MarketingMessageContentMap.AddRange(messagesdb);
            db.SaveChanges();

        }

        /// <summary>
        /// Getting Marketing Message By Id
        /// </summary>
        /// <param name="MarketingMessageID"></param>
        /// <returns></returns>
       public  MarketingMessage GetMarketingMessageById(int MarketingMessageID)
        {
            var db = ObjectContextFactory.Create();
            MarketingMessagesDb marketingMessageDb = db.MarketingMessages.Where(m => m.MarketingMessageID == MarketingMessageID).FirstOrDefault();
            List<int> accountIds = db.MarketingMessageAccountMap.Where(ac => ac.MarketingMessageID == MarketingMessageID).Select(a => a.AccountID).ToList();
            List<MarketingMessageContentMapDb> messageContents = db.MarketingMessageContentMap.Where(m => m.MarketingMessageID == MarketingMessageID).ToList();
            marketingMessageDb.Messages = messageContents;
            marketingMessageDb.AccountIDs = accountIds;
            return Mapper.Map<MarketingMessagesDb, MarketingMessage>(marketingMessageDb);

        }

        /// <summary>
        /// Deleting Marketing Messages
        /// </summary>
        /// <param name="messagesIds"></param>
        public void DeleteMarketingMessage(int[] messagesIds)
        {
            var db = ObjectContextFactory.Create();
            foreach (int id in messagesIds)
            {
                MarketingMessagesDb Message = db.MarketingMessages.Where(i => i.MarketingMessageID == id).SingleOrDefault();
                Message.IsDeleted = true;
                IEnumerable<MarketingMessageAccountMapDb> accounts = db.MarketingMessageAccountMap.Where(i => i.MarketingMessageID == id).ToList();
                IEnumerable<MarketingMessageContentMapDb> contents = db.MarketingMessageContentMap.Where(i => i.MarketingMessageID == id).ToList();
                db.MarketingMessageAccountMap.RemoveRange(accounts);
                db.MarketingMessageContentMap.RemoveRange(contents);
                db.SaveChanges();

            }
            
       }

        /// <summary>
        /// Get All Marketing Messages
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
       public  IEnumerable<MarketingMessage> GetAllMessages(int limit, int pagenumber)
        {
            var skip = (pagenumber - 1) * limit;
            var take = limit;
            var db = ObjectContextFactory.Create();
            var sql_query = @"SELECT m.MarketingMessageID, m.MarketingMessageTitle, m.Status,m.ScheduleFrom,m.ScheduleTo,count(1) MessageCount, m.CreatedDate,COUNT(1) OVER() as TotalCount FROM MarketingMessages m (NOLOCK) 
                            INNER JOIN MarketingMessageContentMap mc (NOLOCK) ON mc.MarketingMessageID = m.MarketingMessageID
                            where m.IsDeleted = 0
                            GROUP BY m.MarketingMessageID, m.MarketingMessageTitle, m.Status, m.CreatedDate,m.ScheduleFrom,m.ScheduleTo
                            ORDER BY m.CreatedDate DESC
                            OFFSET @Skip ROWS
                            FETCH NEXT @Take ROWS ONLY";
            var messgessDb = db.Get<MarketingMessagesDb>(sql_query, new { Skip = skip, Take = take});
            return Mapper.Map<IEnumerable<MarketingMessagesDb>, IEnumerable<MarketingMessage>>(messgessDb);
        }

        /// <summary>
        /// Get All Marketing Contents
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public IEnumerable<MarketingMessageContentMap> GetMarketingMessageConentsByAccount( int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql_query = @"select mc.Content, m.TimeInterval,mc.Subject,mc.Icon
                                from	MarketingMessageContentMap mc (nolock) 
                                inner join MarketingMessages m (nolock) on mc.MarketingMessageID = m.MarketingMessageID
                                inner join MarketingMessageAccountMap am (nolock) on am.MarketingMessageID = m.MarketingMessageID
                                where m.Status = 1002 and  m.IsDeleted = 0 and am.AccountID = @AccountId
                                group by mc.Content,m.TimeInterval,mc.Subject,mc.Icon,mc.MarketingMessageContentMapID
								order by mc.MarketingMessageContentMapID asc";
            var messgessDb = db.Get<MarketingMessageContentMapDb>(sql_query, new { AccountID = accountId });
            return Mapper.Map<IEnumerable<MarketingMessageContentMapDb>, IEnumerable<MarketingMessageContentMap>>(messgessDb);
        }

        /// <summary>
        /// Deleting Marketing Accounts
        /// </summary>
        /// <param name="MarketingMessageID"></param>
       public void DeleteMarketingAccounts(int MarketingMessageID)
        {
            var db = ObjectContextFactory.Create();
            db.MarketingMessageAccountMap.RemoveRange(db.MarketingMessageAccountMap.Where(m => m.MarketingMessageID == MarketingMessageID));
            db.SaveChanges();
        }

        /// <summary>
        /// Deleting Marketing messages
        /// </summary>
        /// <param name="MarketingMessageID"></param>
        public void DeleteMarketingMessageContents(int MarketingMessageID)
        {
            var db = ObjectContextFactory.Create();
            db.MarketingMessageContentMap.RemoveRange(db.MarketingMessageContentMap.Where(m => m.MarketingMessageID == MarketingMessageID));
            db.SaveChanges();
        }

        /// <summary>
        /// Validating the Notification Title is Unque
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsValidMarketingMessage(MarketingMessage message)
        {
            var db = ObjectContextFactory.Create();
            var MessageFound = db.MarketingMessages.Where(c => c.MarketingMessageTitle.Equals(message.MarketingMessageTitle, StringComparison.CurrentCultureIgnoreCase)
                                                     && c.MarketingMessageID != message.MarketingMessageID && c.IsDeleted == false)
                                            .Select(c => c).FirstOrDefault();
            if (MessageFound != null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get All Published Account Names
        /// </summary>
        /// <param name="accountIds"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public IEnumerable<string> GetAllPublishedAccounts(IList<int> accountIds,int messageId,DateTime? from,DateTime? to)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var accountNames = new List<string> { };
                var minDate = from == null?(DateTime)System.Data.SqlTypes.SqlDateTime.MinValue: from;
                var maxDate = to == null ? (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue : to;

                db.QueryStoredProc("[dbo].[MessageCenterValidator]", (reader) =>
                {
                    accountNames = reader.Read<string>().ToList();
                }, new
                {
                    Accounts = accountIds.AsTableValuedParameter("dbo.Contact_List"),
                    MessageId = messageId,
                    From = minDate,
                    To = maxDate
                });

                return accountNames;
            }
                
        }


        public MarketingMessage FindBy(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MarketingMessage> FindAll()
        {
            throw new NotImplementedException();
        }

        public override MarketingMessage FindBy(int id)
        {
            throw new NotImplementedException();
        }

        public override MarketingMessagesDb ConvertToDatabaseType(MarketingMessage domainType, CRMDb context)
        {
            throw new NotImplementedException();
        }

       
        public override MarketingMessage ConvertToDomain(MarketingMessagesDb databaseType)
        {
            throw new NotImplementedException();
        }

    }
}
