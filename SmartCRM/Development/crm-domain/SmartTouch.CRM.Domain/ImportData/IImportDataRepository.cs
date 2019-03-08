using System;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.ImportData
{
    public interface IImportDataRepository : IRepository<ImportData, int>
    {
        int InsertLeadAdapterjob(LeadAdapterJobLogs leadAdapterJobLog,Guid uniqueidentifier,bool updateOnDuplicate,bool isFromImport,int AccountID, int UserID, int DuplicteLogicID,int ownerId,bool IncludeInReports,short leadSourceId);
        IList<int> UploadContacts(List<RawContact> contacts, LeadAdapterJobLogs jobLog, byte importedFrom, Guid uniqueidentifier, 
                                  int AccountID, int leadAdapterAccountMapID,int leadAdapterJobLogID,int? OwnerID);
        IEnumerable<ImportData> FindAll(string name, int limit, int pageNumber, int leadaccountmapId);
        IEnumerable<ImportData> FindAll(string name, int leadaccountmapId);
        ImportDataSettings getImportdataSetting(string uniqueidentifier);
        string GetStates(string statename);
        string GetCountries(string countryname);
        int GetLeadAdapterAccountMap(int AccountId);
        LeadAdapterAndAccountMap GetImportForAccountID(int AccountID);
        IEnumerable<int> OwnerIDforImportedContacts(int AccountID);
        void UpdateLeadAdapterStatus(int jobid, LeadAdapterJobStatus? status);
        void UpdateLeadadaptersFileStatus(int JobLogID,IEnumerable<int> UserIDs,int AccountID, string storageFileName,LeadAdapterErrorStatus errorstatus,int LeadAdapterID);
        void InsertImportTags(int JobLogID, IEnumerable<Tag> Tags);
        List<int> GetImportTags(DateTime dateTime);
        IEnumerable<int> GetLeadAdapterTags(int LeadAdapterAndAccountMapID);

        void InsertImportsData(ImportContactsData contacts);

        List<int> GetImportedContacts(DateTime lastModifiedDate);

        void InsertImportContactEmailStatuses(List<RawContact> contacts);
        void InsertImportCustomFieldData(IList<ImportCustomData> contactCustomData);
        void InsertImportPhoneData(IList<ImportPhoneData> contactPhoneData);


        void MergeDuplicateImportData(string customData, string phoneData,Guid referenceId,Guid contactReferenceId);

        RawContact GetImportContactByDataID(int dataId);

        string GetSourceByJobId(int jobId);

        string GetLeadAdapterTypeByReferenceId(Guid guid);

        List<ContactCustomField> GetImportCustomFieldsByRefID(Guid guid);
        IEnumerable<ImportColumnMappings> GetColumnMappings(int jobId);
        void InsertColumnMappings(IEnumerable<ImportColumnMappings> columnMappings);
        void UpdateLeadAdapterJobLogsWithProcessedFileName(int jobId, string processedFileName);

        void InsertNeverBounceRequest(int accountId, int createdBy, byte entityType, List<int> entityIds, int totalCount);
        IEnumerable<NeverBounceQueue> GetNeverBounceRequests(int accountId,int pageNumber,int pageSize);
        IEnumerable<NeverBounceRequest> GetNeverBounceAcceptedRequests(NeverBounceStatus status);
        IEnumerable<ReportContact> GetContactEmails(NeverBounceEntityTypes entityType, string entityIds);
        void UpdateNeverBounceRequest(NeverBounceRequest request);
        void UpdateRequest(int Id, int userId, NeverBounceStatus status);
        void UpdateNeverBouncePollingResponse(NeverBounceRequest request);
        void InsertNeverBounceResults(IEnumerable<NeverBounceResult> results);
        NeverBounceEmailData GetEmailData(int neverBounceRequestID);
        short GetLeadSourceIdByAccountIdAndLeadAdapterType(int jobId, byte typeId);
    }
}
