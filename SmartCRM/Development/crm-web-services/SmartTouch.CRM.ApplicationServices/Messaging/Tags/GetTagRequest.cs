using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class GetTagRequest : IntegerIdRequest//ServiceRequestBase
    {
        //public string TagName { get; set; }
        public GetTagRequest(int id) : base(id) { }
    }

    public class GetTagResponse : ServiceResponseBase
    {
        public ITagViewModel TagViewModel { get; set; }
    }
}