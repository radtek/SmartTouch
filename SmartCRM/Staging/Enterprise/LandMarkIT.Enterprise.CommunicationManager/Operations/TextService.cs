using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.CommunicationManager.Text;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Extensions;
using LandmarkIT.Enterprise.CommunicationManager.Processors;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    public class TextService : BaseService
    {
        private ITextService GetService(Guid token)
        {
            Logger.Current.Verbose("Request received for getting text-service with token :" + token);
            var service = default(ITextService);
            var provider = TextProvider.Twillio;

            var registration = this.unitOfWork.TextRegistrationsRepository.FirstOrDefault(ft => ft.Guid == token);
            if (registration != null) provider = registration.TextProviderID;

            switch (provider)
            {
                case TextProvider.Twillio:
                    service = new TwilioTextService(token);
                    Logger.Current.Informational("Twillio text service is configured for the above token");
                    break;                
                default:
                    break;
            }
            return service;
        }        

        public SendTextResponse Send(SendTextRequest request)
        {
            Logger.Current.Verbose("Request received for sending a text message");
            SendTextResponse response = GetService(request.TokenGuid).Send(request);
            LogResponse(request, response);
            return response;
        }

        public SendTextResponse SendText(SendTextRequest request)
        {
            var response = default(SendTextResponse);

            try
            {
                Logger.Current.Verbose("Request received for sending a text message");
                if (request.ScheduledTime > DateTime.Now.ToUniversalTime())
                {
                    Logger.Current.Verbose("Request received for sending a text message with scheduled-time");
                    this.QueueMessages(new List<SendTextRequest> { request });
                    Logger.Current.Verbose("Text request was queued successfully");
                    response = new SendTextResponse
                    {
                        //Token = request.TokenGuid,
                        RequestGuid = request.RequestGuid,
                        StatusID = CommunicationStatus.Queued,
                        ServiceResponse = string.Empty
                    };
                }
                else
                {
                    Logger.Current.Verbose("Request received for sending a text message with out scheduled-time");
                    response = Send(request);
                    this.LogResponse(request, response);
                }
            }
            catch (Exception ex)
            {
                var rethrowException = ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_AND_RETHROW_POLICY);
                Logger.Current.Error("Exception occured while sending a text message from text service.", ex);
                if (rethrowException) throw;
            }

            return response;
        }

        public TextRegistrationDb GetTextRegistrationDetails(Guid token)
        {
            TextRegistrationDb registration = unitOfWork.TextRegistrationsRepository.FirstOrDefault(ft => ft.Guid == token);
            return registration;
        }

        private void LogResponse(SendTextRequest request, SendTextResponse response)
        {
            Logger.Current.Verbose("Request received for logging text response");
            //SentText & SentTextDetails Table
            var sentText = new SentTextDb
            {
                Token = request.TokenGuid,
                RequestGuid = request.RequestGuid,
                CreatedDate =DateTime.Now,
            };
            
            unitOfWork.SentTextRepository.Add(sentText);
            unitOfWork.Commit();
             SentTextDb sentTextdb = unitOfWork.SentTextRepository.FirstOrDefault(mr => mr.RequestGuid == request.RequestGuid);
            var sentTextDetail = new SentTextDetailsDb
            {
                TextResponseID = sentTextdb.TextResponseID,
                From = request.From,
                To = request.To.ToPlainString(),
                SenderID = request.SenderId,
                Message = request.Message,
                Status = (byte)response.StatusID,
                ServiceResponse = response.ServiceResponse

            };
            unitOfWork.SentTextDetailsRepository.Add(sentTextDetail);
            unitOfWork.Commit();
            Logger.Current.Verbose("Logging text response was successful");
        }

        private void QueueMessages(List<SendTextRequest> request)
        {
            var batchLoad = 300;
            var queueTime = DateTime.UtcNow;

            var iterations = (request.Count / batchLoad);
            if ((request.Count % batchLoad) > 0) iterations++;

            Logger.Current.Verbose("Request received for queueing list of text requests");

            for (int i = 0; i < iterations; i++)
            {
                var currentIterations = request.Skip(i * batchLoad).Take(batchLoad).ToList();

                currentIterations.ForEach(item =>
                {
                    this.unitOfWork.SentTextRepository.Add(new SentTextDb 
                    {
                        CreatedDate = DateTime.UtcNow,
                        RequestGuid = item.RequestGuid,
                        Token = item.TokenGuid,
                    });
                    unitOfWork.Commit();
                    SentTextDb sentTextdb = unitOfWork.SentTextRepository.Single(mr => mr.RequestGuid == item.RequestGuid);
                    this.unitOfWork.SendTextQueueRepository.Add(new SendTextQueueDb
                    {
                        TokenGuid = item.TokenGuid,
                        RequestGuid = item.RequestGuid,
                        //From = item.From,
                        //PriorityID = item.PriorityID,
                        ScheduledTime = item.ScheduledTime,
                        QueueTime = queueTime,
                        //StatusID = CommunicationStatus.Queued,
                        //ServiceResponse = string.Empty
                    });
                    this.unitOfWork.SentTextDetailsRepository.Add(new SentTextDetailsDb
                    {
                        TextResponseID = sentTextdb.TextResponseID,
                        From = item.From,
                        To = item.To.ToPlainString(),
                        SenderID = item.SenderId,
                        Message = item.Message,
                        Status = (byte)CommunicationStatus.Queued
                        //ServiceResponse = response.ServiceResponse
                    });
                });
                unitOfWork.Commit();
            }
            Logger.Current.Verbose("Queueing text requests was successful");
        }

        public void RemoveScheduledReminder(Guid requestGuid)
        {
            Logger.Current.Verbose("Request received for removing scheduled reminder from SendTextQueue");
            if (requestGuid != null)
            {
                SendTextQueueDb sendTextQueue = this.unitOfWork.SendTextQueueRepository.FirstOrDefault(f => f.RequestGuid == requestGuid);
                if (sendTextQueue != null)
                {
                    this.unitOfWork.SendTextQueueRepository.Delete(sendTextQueue);
                    unitOfWork.Commit();
                }
            }
        }

        public void RemoveAllTextScheduledReminder(Guid requestGuid)
        {
            Logger.Current.Verbose("Request received for removing scheduled reminder from SendTextQueue");
            if (requestGuid != null)
            {
                IEnumerable<SendTextQueueDb> sendTextQueue = this.unitOfWork.SendTextQueueRepository.Where(t => t.RequestGuid == requestGuid).ToList();
                if (sendTextQueue != null)
                {
                    this.unitOfWork.SendTextQueueRepository.RemoveRange(sendTextQueue);
                    unitOfWork.Commit();
                }
            }
        }

        public void UpdateScheduledReminder(Guid requestGuid, SendTextRequest request)
        {
            Logger.Current.Verbose("Request received for updating scheduled reminder from SendTextQueue");
            if (requestGuid != null && request != null)
            {
                SendTextQueueDb sendTextQueue = this.unitOfWork.SendTextQueueRepository.FirstOrDefault(f => f.RequestGuid == requestGuid);
                if (sendTextQueue != null)
                {
                    sendTextQueue.QueueTime = DateTime.UtcNow;
                    sendTextQueue.ScheduledTime = request.ScheduledTime;
                }
                SentTextDetailsDb sendTextDetail = new SentTextDetailsDb();
                SentTextDb textResponse = this.unitOfWork.SentTextRepository.FirstOrDefault(f => f.RequestGuid == requestGuid);
                if(textResponse != null)
                    sendTextDetail = this.unitOfWork.SentTextDetailsRepository.FirstOrDefault(f => f.TextResponseID == textResponse.TextResponseID);
                if (sendTextDetail != null)
                {
                    sendTextDetail.From = request.From;
                    sendTextDetail.Message = request.Message;
                    sendTextDetail.SenderID = request.SenderId;
                    sendTextDetail.To = request.To.FirstOrDefault();
                }
                if (sendTextQueue == null && sendTextDetail != null)
                {
                    this.unitOfWork.SendTextQueueRepository.Add(new SendTextQueueDb
                    {
                        TokenGuid = request.TokenGuid,
                        RequestGuid = request.RequestGuid,
                        //From = item.From,
                        //PriorityID = item.PriorityID,
                        ScheduledTime = request.ScheduledTime,
                        QueueTime = DateTime.UtcNow,
                        //StatusID = CommunicationStatus.Queued,
                        //ServiceResponse = string.Empty
                    });
                }
                unitOfWork.Commit();
            }
        
        }
    }
}
