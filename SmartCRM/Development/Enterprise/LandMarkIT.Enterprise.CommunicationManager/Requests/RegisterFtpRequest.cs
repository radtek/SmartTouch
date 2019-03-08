using LandmarkIT.Enterprise.Common;
using System.Runtime.Serialization;

namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
    [DataContract(Namespace = NameSpaces.CONTRACT_V1)]
    public class RegisterFtpRequest
    {
        [DataMember]
        public string Host { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public int? Port { get; set; }
        [DataMember]
        public bool EnableSsl { get; set; }
    }
}
