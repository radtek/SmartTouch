using LandmarkIT.Enterprise.Common;
using System.Runtime.Serialization;
using System;

namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
     [DataContract(Namespace = NameSpaces.CONTRACT_V1)]
    public class RegisterTextRequest
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public Guid RequestGuid { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Token { get; set; }
        public TextProvider TextProviderID { get; set; }
    }
}
