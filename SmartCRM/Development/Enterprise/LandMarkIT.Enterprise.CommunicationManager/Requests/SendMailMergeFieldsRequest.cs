using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
    public class SendMailMergeFieldsRequest
    {
        public int ContactId { get; set; }
        public string FieldCode { get; set; }
        public string FieldValue { get; set; }
        public string Email { get; set; }
    }
}
