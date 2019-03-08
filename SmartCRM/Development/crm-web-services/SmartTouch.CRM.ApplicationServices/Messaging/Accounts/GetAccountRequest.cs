using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAccountRequest : IntegerIdRequest
    {
        public GetAccountRequest(int id) : base(id) { }
        public bool RequestBySTAdmin { get; set; }
    }

    public class GetAccountNameRequest : ServiceRequestBase
    {
        public string name { get; set; }
    }

    public class GetAccountIdRequest : ServiceRequestBase
    {
        public int accountId { get; set; }
    }

    public class GetAccountResponse : ServiceResponseBase
    {
        public AccountViewModel AccountViewModel { get; set; }
    }
    public class GetAccountIdByContactIdRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
    }
    public class GetAccountIdByContactIdResponse: ServiceResponseBase
    {
        public int AccountId { get; set; }
    }
    public class GetAccountDropboxKeyResponse : ServiceResponseBase
    {
        public string DropboxKey { get; set; }
    }
}
