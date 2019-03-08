using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Accounts;

namespace SmartTouch.CRM.Domain.LeadAdapters
{
    public interface ILeadAdaptersRepository : IRepository<LeadAdapterAndAccountMap, int>
    {
        IEnumerable<LeadAdapterAndAccountMap> GetLeadAdapters(int accountID);
        IEnumerable<LeadAdapterAndAccountMap> GetAllLeadAdapters();

        void DeleteLeadAdapter(int leadAdapterID);
        LeadAdapterAndAccountMap GetLeadAdapter(int accountID, LeadAdapterTypes leadAdapterType);
        LeadAdapterAndAccountMap GetLeadAdapterByID(int leadAdapterAndAccountMapID);

        IEnumerable<LeadAdapterAndAccountMap> FindAll(string name, int limit, int pageNumber, int accountID);
        IEnumerable<LeadAdapterAndAccountMap> FindAll(string name, int accountID);
        bool IsDuplicateLeadAdapter(LeadAdapterTypes leadAdapterType, int accountID, int leadAdapterAndAccountMapId);
        IEnumerable<LeadAdapterJobLogs> FindLeadAdapterJobLogAll(int limit, int pageNumber, int leadAdapterAndAccountMapID);
        IEnumerable<LeadAdapterJobLogs> FindLeadAdapterJobLogAll(int leadAdapterAndAccountMapID);
        void UpdateFacebookLeadAdapter(FacebookLeadAdapter fla);
        DateTime? GetLeadAdapterJobLogsBy(int accountId, byte leadAdapterType);
        void CreateLeadAdapterFolders(string accountID, string leadAdapterBaseDirectory);

        void InsertFacebookLeadAdapter(FacebookLeadAdapter fla);
        void InsertFacebookLeadGen(FacebookLeadGen fbLead);
        void UpdateFacebookLeadGen(FacebookLeadGen fbLead);
        bool IsDuplicateFacebookAdapter(int accountId, int leadadapterID, string name);
        IEnumerable<FacebookLeadGen> GetFacebookLeadGens(int accountMapID);
        Account GetFacebookApp(int accountId);
        bool HasFacebookFields(int accountId);
        void UpdateFacebookPageToken(string extendedToken, int accountMapID);
        void UpdateLeadAdapterStatus(int accountMapID, LeadAdapterErrorStatus status, LeadAdapterServiceStatus serviceStatus);

        short GetDropdownValueID(int AccountID, DropdownValueTypes DropdownValueTypeID);
        bool isLinkedToWorkflows(int LeadAdapterID);
        string GetLeadAdapterSubmittedDataByID(int JobLogDetailID);
        bool isLeadAdapterAlreadyConfigured(int AccountID, LeadAdapterTypes leadAdapterType);

        IEnumerable<LeadAdapterAndAccountMap> GetImportLeads();

        IEnumerable<LeadAdapterAndAccountMap> GetLeadData(string BuilderNumber, string CommunityNumber, LeadAdapterTypes leadAdapterTypes, IEnumerable<Guid> accountId);
        IEnumerable<LeadAdapterAndAccountMap> GetEmptyCommunities(string BuilderNumber, LeadAdapterTypes leadAdapterType, IEnumerable<Guid> guids);
        bool IsCommunityExists(string CommunityNumber, LeadAdapterTypes leadAdapterTypes,int accountId);
        void UpdateProcessedDate(int accountMapId);
        int ImpotedLeadByJobId(int jobId);
        byte GetLeadAdapterTypeByJobId(int jobId);
        IEnumerable<LeadAdapterType> GetAllLeadadapterTypes();
    }
}
