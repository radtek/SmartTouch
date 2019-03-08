using SmartTouch.CRM.ApplicationServices.ViewModels;


namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{
    public class GetImportForAccountRequest : ServiceRequestBase
    {
        
    }

    public class GetImportForAccountResponse : ServiceResponseBase
    {
        public LeadAdapterViewModel Import { get; set; }
    }
}
