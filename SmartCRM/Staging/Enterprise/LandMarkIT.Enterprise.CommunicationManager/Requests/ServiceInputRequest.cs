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
    public abstract class ServiceInputRequest
    {
        [DataMember]
        public Guid RequestGuid { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Address { get; set; }
    }
}
