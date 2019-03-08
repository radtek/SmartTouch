using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters;
using SmartTouch.CRM.Domain.LeadAdapters;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ILeadAdapterService
    {
        //GetLeadAdapterListResponse GetAllLeadAdapters(GetLeadAdapterListRequest request);
        //SearchLeadAdaptersResponse GetAllLeadAdapters(SearchLeadAdaptersRequest request);
        InsertLeadAdapterResponse InsertFacebookLeadAdapter(InsertLeadAdapterRequest request);
        UpdateLeadAdapterResponse UpdateFacebookLeadAdapter(UpdateLeadAdapterRequest request);
        InsertFacebookLeadGenResponse InsertFacebookLeadGen(InsertFacebookLeadGenRequest request);
        GetFacebookLeadGensResponse GetFacebookLeadGens(GetFacebookLeadGensRequest request);
        GetFacebookAppResponse GetFacebookApp(GetFacebookAppRequest request);
        DeleteLeadAdapterResponse DeleteLeadAdapter(DeleteLeadAdapterRequest request);
        InsertLeadAdapterResponse InsertLeadAdapter(InsertLeadAdapterRequest request);
        UpdateLeadAdapterResponse UpdateLeadAdapter(UpdateLeadAdapterRequest request);
        GetLeadAdapterResponse GetLeadAdapter(GetLeadAdapterRequest request);
        GetLeadAdapterListResponse GetAllLeadAdapters(GetLeadAdapterListRequest request);
        GetViewLeadAdapterListResponse GetAllViewLeadAdapters(GetViewLeadAdapterListRequest request);
        GetLeadAdapterJobLogDetailsListResponse GetAllLeadAdaptersJobLogDetails(GetLeadAdapterJobLogDetailsListRequest request);
        GetLeadAdapterSubmissionResponse GetLeadAdapterSubmission(GetLeadAdapterSubmissionRequest request);
        LeadAdapterData GetLeadAdapterData(int? leadadaptermapId);
        string GetLeadAdapterName(int? leadadaptermapId);
        string GetFaceBookHostNameByContactId(int contactId);
        IEnumerable<LeadAdapterType> GetAllLeadadapterTypes();
    }
}
