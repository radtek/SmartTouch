using AutoMapper;
using SmartTouch.CRM.Infrastructure.UnitOfWork;

using SmartTouch.CRM.ApplicationServices.Messaging.MarketingMessages;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.MarketingMessageCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
   public class MarketingMessageService: IMarketingMessageService
    {
        readonly IMarketingMessagesRopository marketingMessagesRopository;
        readonly IUnitOfWork unitOfWork;

        public MarketingMessageService(IMarketingMessagesRopository marketingMessagesRopository, IUnitOfWork unitOfWork)
        {
            if (marketingMessagesRopository == null)
                throw new ArgumentNullException("marketingMessagesRopository");

            this.marketingMessagesRopository = marketingMessagesRopository;
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get All Marketing Messages
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetAllMarketingMessagesResponse GetAllMarketingMessages(GetAllMarketingMessagesRequest request)
        {
            GetAllMarketingMessagesResponse response = new GetAllMarketingMessagesResponse();
            IEnumerable<MarketingMessage>  marketingMessages = marketingMessagesRopository.GetAllMessages(request.Limit, request.PageNumber);

            IEnumerable<MarketingMessagesViewModel> marketingMessagesList = MapDomainToVM(marketingMessages);
            response.MarketingMessagesViewModel = marketingMessagesList;
            response.TotalHits = marketingMessagesList.IsAny()? marketingMessagesList.Select(s => s.TotalCount).FirstOrDefault():0;
            return response;
        }

        /// <summary>
        /// Mapping Domain to ViewModel
        /// </summary>
        /// <param name="marketingmessagesDomain"></param>
        /// <returns></returns>
        private IEnumerable<MarketingMessagesViewModel> MapDomainToVM(IEnumerable<MarketingMessage> marketingmessagesDomain)
        {
            List<MarketingMessagesViewModel> marketingMessages = new List<MarketingMessagesViewModel>();
            if (marketingmessagesDomain.IsAny())
            {
                foreach (var message in marketingmessagesDomain)
                {
                    MarketingMessagesViewModel details = new MarketingMessagesViewModel();
                    details.MarketingMessageID = message.MarketingMessageID;
                    details.MarketingMessageTitle = message.MarketingMessageTitle;
                    details.TimeInterval = message.TimeInterval;
                    details.Status = message.Status;
                    details.SelectedBy = message.SelectedBy;
                    details.IsDeleted = message.IsDeleted;
                    details.MessageCount = message.MessageCount;
                    details.CreatedDate = message.CreatedDate;
                    details.ScheduleFrom = message.ScheduleFrom;
                    details.ScheduleTo = message.ScheduleTo;
                    details.TotalCount = message.TotalCount;
                    marketingMessages.Add(details);
                }
            }
            return marketingMessages;
        }

        /// <summary>
        /// Get All Marketing Message Contents
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetAllMarketingMessageContentResponse GetAllMarketingMessageContents(GetAllMarketingMessageContentRequest request)
        {
            GetAllMarketingMessageContentResponse response = new GetAllMarketingMessageContentResponse();
            IEnumerable<MarketingMessageContentMap> marketingMessageContents = marketingMessagesRopository.GetMarketingMessageConentsByAccount(request.AccountID);
            response.marketingMessagesViewModel = Mapper.Map<IEnumerable<MarketingMessageContentMap>,IEnumerable<MarketingMessageContentMapViewModel>>(marketingMessageContents) ;
            return response;
        }



        /// <summary>
        /// Inserting Marketing Message
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public InsertMarketingMessageResponse insertMarketingMessage(InsertMarketingMessageRequest request)
        {
            InsertMarketingMessageResponse response = new InsertMarketingMessageResponse();

            List<MarketingMessageAccountMapViewModel> accountViewModellist = new List<MarketingMessageAccountMapViewModel>();
            List<MarketingMessageContentMapViewModel> messageViewModelList = new List<MarketingMessageContentMapViewModel>();

            MarketingMessage marketingMessage = Mapper.Map<MarketingMessagesViewModel, MarketingMessage>(request.marketingMessageViewModel);
            if (marketingMessage.MarketingMessageTitle.Length > 75)
                throw new UnsupportedOperationException("[|Notification Title  Is Maximum 75 characters.|]");
            foreach(MarketingMessageContentMapViewModel messageSubject in request.marketingMessageViewModel.Messages)
            {
                if (messageSubject.Subject.Length > 75)
                    throw new UnsupportedOperationException("[|Message Subject  Is Maximum 75 characters.|]");

            }
            bool isMessageTileUniue = marketingMessagesRopository.IsValidMarketingMessage(marketingMessage);
            if (!isMessageTileUniue)
            {
                Logger.Current.Verbose("Duplicate Notification Title is Identified," + marketingMessage.MarketingMessageTitle);
                var notifation = "[|Notification with Title|] \"" + marketingMessage.MarketingMessageTitle + "\" [|already exists.|] " + "[|Please choose a different Title|]";
                throw new UnsupportedOperationException(notifation);
            }

            if(request.marketingMessageViewModel.Status == (short)MarketingMessageStatus.Publish)
            {
                IEnumerable<string>  accountMessage = marketingMessagesRopository.GetAllPublishedAccounts(request.marketingMessageViewModel.AccountIDs,0,request.marketingMessageViewModel.ScheduleFrom, request.marketingMessageViewModel.ScheduleTo);
                if(accountMessage.IsAny())
                {
                    string finalaccountMessage = string.Join(",", accountMessage);
                    var notifation = "[| Message for " + finalaccountMessage + "  already exits. Please exclude " + finalaccountMessage + " in selection. |]";
                    throw new UnsupportedOperationException(notifation);
                }
            }

           int marketingMessageID = marketingMessagesRopository.InsertMarketingMessage(marketingMessage);

            foreach (int accountid in request.marketingMessageViewModel.AccountIDs)
            {
                MarketingMessageAccountMapViewModel accountviewmodel = new MarketingMessageAccountMapViewModel();
                accountviewmodel.AccountID = accountid;
                accountviewmodel.MarketingMessageID = marketingMessageID;
                accountViewModellist.Add(accountviewmodel);
               
            }
           
            marketingMessage.MarketingMessageAccountMaps = Mapper.Map<IEnumerable<MarketingMessageAccountMapViewModel>, IEnumerable<MarketingMessageAccountMap>>(accountViewModellist.ToArray());
            if (marketingMessage.MarketingMessageAccountMaps != null)
            {
                marketingMessagesRopository.InsertMarketingAccounts(marketingMessage.MarketingMessageAccountMaps);
            }

            foreach (MarketingMessageContentMapViewModel content in request.marketingMessageViewModel.Messages)
            {
                MarketingMessageContentMapViewModel messageViewmodel = new MarketingMessageContentMapViewModel();
                if(content.IsDeleted == false)
                {
                    messageViewmodel.Subject = content.Subject;
                    messageViewmodel.Icon = content.Icon;
                    messageViewmodel.Content = content.Content;
                    messageViewmodel.MarketingMessageID = marketingMessageID;
                    messageViewModelList.Add(messageViewmodel);
                }
                
            }

            marketingMessage.MarketingMessageContentMaps = Mapper.Map<IEnumerable<MarketingMessageContentMapViewModel>, IEnumerable<MarketingMessageContentMap>>(messageViewModelList.ToArray());
            if (marketingMessage.MarketingMessageContentMaps != null)
            {
                marketingMessagesRopository.InsertMarketingMessageContents(marketingMessage.MarketingMessageContentMaps);
            }
            return response;
        }

        /// <summary>
        /// Updating Marketing Message
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public UpdateMarketingMessageResponse updateMarketingMessage(UpdateMarketingMessageRequest request)
        {
            UpdateMarketingMessageResponse response = new UpdateMarketingMessageResponse();
            List<MarketingMessageAccountMapViewModel> accountViewModellist = new List<MarketingMessageAccountMapViewModel>();
            List<MarketingMessageContentMapViewModel> messageViewModelList = new List<MarketingMessageContentMapViewModel>();

            MarketingMessage marketingMessage = Mapper.Map<MarketingMessagesViewModel, MarketingMessage>(request.marketingMessagesViewModel);
            if (marketingMessage.MarketingMessageTitle.Length > 75)
                throw new UnsupportedOperationException("[|Notification Title  Is Maximum 75 characters.|]");
            foreach (MarketingMessageContentMapViewModel messageSubject in request.marketingMessagesViewModel.Messages)
            {
                if (messageSubject.Subject.Length > 75)
                    throw new UnsupportedOperationException("[|Message Subject  Is Maximum 75 characters.|]");

            }
            bool isMessageTileUniue = marketingMessagesRopository.IsValidMarketingMessage(marketingMessage);
            if (!isMessageTileUniue)
            {
                Logger.Current.Verbose("Duplicate Notification Title is Identified," + marketingMessage.MarketingMessageTitle);
                var notifation = "[|Notification with Title|] \"" + marketingMessage.MarketingMessageTitle + "\" [|already exists.|] " + "[|Please choose a different Title|]";
                throw new UnsupportedOperationException(notifation);
            }
            if (request.marketingMessagesViewModel.Status == (short)MarketingMessageStatus.Publish || request.marketingMessagesViewModel.Status == (short)MarketingMessageStatus.Active)
            {
                IEnumerable<string> accountMessage = marketingMessagesRopository.GetAllPublishedAccounts(request.marketingMessagesViewModel.AccountIDs,request.marketingMessagesViewModel.MarketingMessageID,request.marketingMessagesViewModel.ScheduleFrom,request.marketingMessagesViewModel.ScheduleTo);
                if(accountMessage.IsAny())
                {
                    string finalaccountMessage = string.Join(",", accountMessage);
                    var notifation = "[| Message for " + finalaccountMessage + "  already exits. Please exclude " + finalaccountMessage + " in selection. |]";
                    throw new UnsupportedOperationException(notifation);
                }
            }
            marketingMessagesRopository.UpdateMarketingMessage(marketingMessage);
            
            foreach (int accountid in request.marketingMessagesViewModel.AccountIDs)
            {
                MarketingMessageAccountMapViewModel accountviewmodel = new MarketingMessageAccountMapViewModel();
                accountviewmodel.AccountID = accountid;
                accountviewmodel.MarketingMessageID = request.marketingMessagesViewModel.MarketingMessageID;
                accountViewModellist.Add(accountviewmodel);
            }

            marketingMessage.MarketingMessageAccountMaps = Mapper.Map<IEnumerable<MarketingMessageAccountMapViewModel>, IEnumerable<MarketingMessageAccountMap>>(accountViewModellist.ToArray());
            if (marketingMessage.MarketingMessageAccountMaps != null)
            {
                marketingMessagesRopository.DeleteMarketingAccounts(request.marketingMessagesViewModel.MarketingMessageID);
                marketingMessagesRopository.InsertMarketingAccounts(marketingMessage.MarketingMessageAccountMaps);
            }

            foreach (MarketingMessageContentMapViewModel content in request.marketingMessagesViewModel.Messages)
            {
                    MarketingMessageContentMapViewModel messageViewmodel = new MarketingMessageContentMapViewModel();
                    if(content.IsDeleted == false)
                    {
                        messageViewmodel.Subject = content.Subject;
                        messageViewmodel.Icon = content.Icon;
                        messageViewmodel.Content = content.Content;
                        messageViewmodel.MarketingMessageID = marketingMessage.MarketingMessageID;
                        messageViewModelList.Add(messageViewmodel);
                    }
            }

            marketingMessage.MarketingMessageContentMaps = Mapper.Map<IEnumerable<MarketingMessageContentMapViewModel>, IEnumerable<MarketingMessageContentMap>>(messageViewModelList.ToArray());
            if (marketingMessage.MarketingMessageContentMaps != null)
            {
                marketingMessagesRopository.DeleteMarketingMessageContents(marketingMessage.MarketingMessageID);
                marketingMessagesRopository.InsertMarketingMessageContents(marketingMessage.MarketingMessageContentMaps);
            }
            return response;
        }

        /// <summary>
        /// Getting Marketing Message By Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetMarketingMessageResponseById GetMarketingMessageById(GetMarketingMessageRequestById request)
        {
            GetMarketingMessageResponseById response = new GetMarketingMessageResponseById();
            MarketingMessage messageViewmodel = marketingMessagesRopository.GetMarketingMessageById(request.MarketingMessageID);

            MarketingMessagesViewModel message = Mapper.Map<MarketingMessage, MarketingMessagesViewModel>(messageViewmodel);
            response.marketingMessagesViewModel = message;
            return response;
        }

        /// <summary>
        /// Deleting the marketing Messages
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DeleteMarketingMessageResponse deleteMarketingMessage(DeleteMarketingMessageRequest request)
        {
            DeleteMarketingMessageResponse response = new DeleteMarketingMessageResponse();
            marketingMessagesRopository.DeleteMarketingMessage(request.MessageIds);
            return response;
        }

    }
}
