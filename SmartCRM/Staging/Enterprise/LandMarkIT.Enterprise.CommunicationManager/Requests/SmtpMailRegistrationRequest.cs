using LandmarkIT.Enterprise.Common;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using System.Runtime.Serialization;

namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
    [DataContract(Namespace = NameSpaces.CONTRACT_V1)]
    public class SmtpMailRegistrationRequest : ServiceInputRequest
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public int? Port { get; set; }
        [DataMember]
        public bool IsSSLEnabled { get; set; }
    }
}
