using System;

namespace LandmarkIT.Enterprise.CommunicationManager.Responses
{
    public class SendTextResponse
    {
        public Guid Token { get; set; }
        public Guid RequestGuid { get; set; }
        public CommunicationStatus StatusID { get; set; }
        public string ServiceResponse { get; set; }
        public string Message { get; set; }
    }
}
