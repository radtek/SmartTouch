using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System.ComponentModel;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class GetWorkflowListRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public short Status { get; set; }
        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }
    }


    public class GetWorkflowListResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<WorkFlowViewModel> Workflows { get; set; }
    }
}
