using LandmarkIT.Enterprise.Common;
using System;
using System.Runtime.Serialization;

namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
    [DataContract(Namespace = NameSpaces.CONTRACT_V1)]
    public class RegisterMailRequest
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Host { get; set; }
        [DataMember]
        public Guid RequestGuid { get; set; }
        [DataMember]
        public string APIKey { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public int? Port { get; set; }
        [DataMember]
        public bool IsSSLEnabled { get; set; }
        [DataMember]
        public string VMTA { get; set; }
        [DataMember]
        public string SenderDomain { get; set; }
        [DataMember]
        public string ImageDomain { get; set; }
        [DataMember]
        public MailProvider MailProviderID { get; set; }
        [DataMember]
        public string MailChimpListID { get; set; }
    }
}
