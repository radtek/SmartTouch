using LandmarkIT.Enterprise.Common;
using System.Runtime.Serialization;

namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
    [DataContract(Namespace = NameSpaces.CONTRACT_V1)]
    public class DropboxRegistrationRequest : ServiceInputRequest
    {
        [DataMember]
        public string ApiKey { get; set; }
        [DataMember]
        public string AppSecret { get; set; }
    }
}
