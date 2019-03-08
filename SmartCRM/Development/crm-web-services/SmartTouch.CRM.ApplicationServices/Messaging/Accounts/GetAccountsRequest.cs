using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;
using System.ComponentModel;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAccountsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public byte Status { get; set; }
        public string SortField { get; set; }
        public byte Id { get; set; }
        public ListSortDirection SortDirection { get; set; }
    }

    public class GetAccountsResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<AccountViewModel> Accounts { get; set; }
    }

    public class GetAccountListResponse : ServiceResponseBase
    {        
        public IEnumerable<AccountListViewModel> Accounts { get; set; }
    } 
}
