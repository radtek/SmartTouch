using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using S = SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SmartTouch.CRM.ApplicationServices.ObjectMappers;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class ImportDataService : IImportDataService
    {
        readonly IAdvancedSearchService advancedSearchService;
        //readonly IUnitOfWork unitOfWork;
        readonly IImportDataRepository importRepository;


        public ImportDataService(IAdvancedSearchService advancedSearchService, IImportDataRepository importRepository)
        {
            this.advancedSearchService = advancedSearchService;
            //this.unitOfWork = unitOfWork;
            this.importRepository = importRepository;
        }

        public GetContactEmailsResponse GetContactEmails(GetContactEmailsRequest request)
        {
            Logger.Current.Informational("Request received to fetch contact emails (ImportDataService)");
            GetContactEmailsResponse response = new GetContactEmailsResponse();
            if (request.EntityType == NeverBounceEntityTypes.Imports || request.EntityType == NeverBounceEntityTypes.Tags)
                response.Contacts = importRepository.GetContactEmails(request.EntityType, request.EntityIds);
            else if (request.EntityType == NeverBounceEntityTypes.SavedSearches)
            {
                List<ReportContact> reportContacts = new List<ReportContact>();
                if (!string.IsNullOrEmpty(request.EntityIds))
                {
                    try
                    {
                        List<string> Ids = request.EntityIds.Split(',').ToList();
                        Logger.Current.Informational("Running SavedSearches " + request.EntityIds);
                        Ids.ForEach(f =>
                        {
                            var task = Task.Run(() => advancedSearchService.GetContactEmails(new S.GetContactEmailsRequest() { AccountId = request.AccountId, SearchDefinitionID = int.Parse(f) }));
                            var contacts = MapToReportContacts(task.Result);
                            if (contacts != null && contacts.Any())
                            {
                                Logger.Current.Informational("Count of contacts found for savedsearch " + f + " is " + contacts.Count);
                                contacts = contacts.Where(w => !string.IsNullOrEmpty(w.email)).ToList();
                                Logger.Current.Informational("No of contacts with valid emails found : " + contacts.Count);
                                reportContacts.AddRange(contacts);
                            }
                        });
                        reportContacts = reportContacts.Distinct().ToList();
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An error occured while fetching data from elasticsearch", ex);
                        throw;
                    }
                }
                response.Contacts = reportContacts;
            }
            else
                response.Contacts = null;
            return response;
        }

        private List<ReportContact> MapToReportContacts(List<Contact> contacts)
        {
            List<ReportContact> reportContacts = new List<ReportContact>();
            if (contacts != null && contacts.Any())
            {
                foreach (var contact in contacts)
                {
                    ReportContact con = new ReportContact();
                    con.contactID = contact.Id;
                    var primaryEmail = contact.Emails.Where(w => w.IsPrimary).FirstOrDefault();
                    if (primaryEmail != null)
                    {
                        con.email = primaryEmail.EmailId;
                        con.ContactEmailID = primaryEmail.ContactEmailID;
                    }
                    reportContacts.Add(con);
                }
            }
            return reportContacts;
        }
    }
}
