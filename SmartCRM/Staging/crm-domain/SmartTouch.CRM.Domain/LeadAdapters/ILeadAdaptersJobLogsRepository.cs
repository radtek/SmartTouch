using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.LeadAdapters
{
    public interface ILeadAdaptersJobLogsRepository : IRepository<LeadAdapterJobLogs, int>
    {
        IEnumerable<LeadAdapterJobLogs> GetLeadAdapterJobDetails(int leadAdapterJobID);
        IEnumerable<LeadAdapterJobLogDetails> FindLeadAdapterJobLogDetailsAll(int limit, int pageNumber, int leadAdapterAndAccountMapID, bool status);
        IEnumerable<LeadAdapterJobLogDetails> FindLeadAdapterJobLogDetailsAll(int leadAdapterAndAccountMapID, bool status);
        IEnumerable<LeadAdapterJobLogs> FindLeadAdapterJobLogAll(int limit, int pageNumber, int leadAdapterAndAccountMapID);
        IEnumerable<LeadAdapterJobLogs> FindLeadAdapterJobLogAll(int leadAdapterAndAccountMapID);
        int FindLeadAdapterJobLogDetailsCount(int leadAdapterAndAccountMapID, bool status);
        LeadAdapterData GetLeadAdapterSubmittedData(int? leadAdapterId);
        string GetLeadAdapterName(int? leadAdapterTypeId);
        string GetFaceBookHostName(int contactId);
    }
}
