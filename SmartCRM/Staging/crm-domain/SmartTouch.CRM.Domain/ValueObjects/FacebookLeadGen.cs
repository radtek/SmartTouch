using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class FacebookLeadGen : ValueObjectBase
    {
        public int FacebookLeadGenID { get; set; }
        public Int64 AdGroupId { get; set; }
        public Int64 AdID { get; set; }
        public Int64 LeadGenID { get; set; }
        public Int64 PageID { get; set; }
        public Int64 FormID { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RawData { get; set; }
        public string Remarks { get; set; }

        public string PageAccessToken { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
