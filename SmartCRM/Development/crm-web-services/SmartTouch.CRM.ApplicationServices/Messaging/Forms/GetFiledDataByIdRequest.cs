using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Fields;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetFiledDataByIdRequest : ServiceRequestBase
    {
        public int FieldId { get; set; }
    }
    public class GetFiledDataByIdResponce : ServiceResponseBase
    {
        public Field Field { get; set; }
    }    
}
