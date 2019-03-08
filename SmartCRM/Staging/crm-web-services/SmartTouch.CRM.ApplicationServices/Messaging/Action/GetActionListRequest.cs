using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using DA = SmartTouch.CRM.Domain.Actions;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class GetActionListRequest : ServiceRequestBase
    {
            public int Id { get; set; }
            public bool IsStAdmin { get; set; }
            public int PageNumber { get; set; }
            public int Limit { get; set; }
            public string Name { get; set; }
            public string Filter { get; set; }
            public bool IsDashboard { get; set; }
            public string SortField { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public ListSortDirection SortDirection { get; set; }
            public string FilterByActionType { get; set; }
            public int[] UserIds { get; set; }
    }

    public class GetActionListResponse : ServiceResponseBase
    {
            //public ActionListViewModel ActionListViewModel { get; set; }
        public IEnumerable<ActionViewModel> ActionListViewModel { get; set; }
        public int TotalHits { get; set; }
    }
    
}
