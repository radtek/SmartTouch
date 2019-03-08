using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class GetConditionsRequest :IntegerIdRequest
    {
        //public int ScoreCategoriesID { get; private set; }

        //public GetConditionsRequest(int scorecategoryID)
        //{
        //    this.ScoreCategoriesID = ScoreCategoriesID;
        //}
        public GetConditionsRequest(int categortID) : base(categortID) { }
    }

    public class GetConditionsResponse : ServiceResponseBase
    {
        public GetConditionsResponse() { }

        //Name, Code
        public IEnumerable<dynamic> Conditions { get; set; }
    }
}
