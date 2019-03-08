using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using Twilio;
using LandmarkIT.Enterprise.CommunicationManager.Processors;
using LandmarkIT.Enterprise.CommunicationManager.Repositories;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using System.Linq;
using LandmarkIT.Enterprise.Extensions;

namespace LandmarkIT.Enterprise.CommunicationManager.Text
{
    public class TwilioTextService : ITextService
    {
        #region Variables
        private IUnitOfWork unitOfWork = default(IUnitOfWork); TwilioRestClient twilioRestClient = default(TwilioRestClient);
        private TextRegistrationDb registration = default(TextRegistrationDb);
        private short parallelLoad = default(short);
        #endregion
        public TwilioTextService(Guid token, short parallelLoad = 30)
        {
            this.unitOfWork = new EfUnitOfWork();
            this.parallelLoad = parallelLoad;
            registration = this.unitOfWork.TextRegistrationsRepository.Single(mr => mr.Guid == token);
            twilioRestClient = new TwilioRestClient(registration.APIKey, registration.Token);
            Logger.Current.Verbose("Request received for sending a text message through Twilio service");
        }

        public SendTextResponse Send(SendTextRequest request)
        {
            Logger.Current.Verbose("Sending text message through twilio service");
            var response = new SendTextResponse { Token = registration.Guid,RequestGuid = request.RequestGuid ,Message = request.Message};
            if (request != null && request.To != null && request.To.Count > 0)
            {
                for (int i = 0; i < request.To.Count; i++)
                {
                    try
                    {
                        string body = request.Message;
                        if (request.OwnerNumbers.IsAny())
                        {
                            string owner = request.OwnerNumbers.Where(o => o.ContactNumber == request.To[i]).Select(s => s.OwnerNumber).FirstOrDefault();
                            body = body + "\nReply to - " + owner;
                        }
                        var twilioResponse = twilioRestClient.SendMessage(string.Concat("+1", request.From), string.Concat("+1", request.To[i]), body);
                        response.StatusID = (twilioResponse.Status != null) ? CommunicationStatus.Success : CommunicationStatus.Failed;
                        if (twilioResponse.Body == null)
                        {
                            response.ServiceResponse = twilioResponse.RestException.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An error occured while sending text message " + ex);
                        response.ServiceResponse = ex.Message;
                        response.StatusID = CommunicationStatus.Failed;
                        ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: new object[] { request });
                    }
                }
            }
            else
            {
                response.StatusID = CommunicationStatus.Rejected;
            }
            return response;
        }
    }
}
