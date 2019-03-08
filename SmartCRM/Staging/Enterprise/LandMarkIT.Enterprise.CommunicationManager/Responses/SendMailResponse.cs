using System;
using System.Collections.Generic;

namespace LandmarkIT.Enterprise.CommunicationManager.Responses
{
    public class SendMailResponse
    {
        public Guid Token { get; set; }
        public Guid RequestGuid { get; set; }        
        public CommunicationStatus StatusID { get; set; }
        public string ServiceResponse { get; set; }
        public IEnumerable<string> FailedRecipients { get; set; }
    }
}
