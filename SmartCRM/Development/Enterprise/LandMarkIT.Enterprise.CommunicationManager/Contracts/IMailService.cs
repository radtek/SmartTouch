using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.Contracts
{
    public interface IMailService
    {
        SendMailResponse Send(SendMailRequest request);
        Task<SendMailResponse> SendAsync(SendMailRequest request);
        List<SendMailResponse> Send(List<SendMailRequest> request);
        Task<List<SendMailResponse>> SendAsync(List<SendMailRequest> request);
    }
}
