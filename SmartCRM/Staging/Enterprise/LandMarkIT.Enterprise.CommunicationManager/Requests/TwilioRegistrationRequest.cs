using LandmarkIT.Enterprise.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
     [DataContract(Namespace = NameSpaces.CONTRACT_V1)]
    public class TwilioRegistrationRequest : ServiceInputRequest
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public string Key { get; set; }
    }
}
