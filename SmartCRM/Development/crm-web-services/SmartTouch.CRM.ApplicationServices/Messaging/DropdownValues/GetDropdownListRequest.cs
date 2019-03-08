using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues
{
    public class GetDropdownListRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public byte Status { get; set; }
        public int AccountID { get; set; }
        public int Id { get; set; }
   }
    public class GetDropdownListResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<DropdownViewModel> DropdownValuesViewModel { get; set; }
    }
}
