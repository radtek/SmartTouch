using System;
using System.Collections.Generic;
using System.Linq;

using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.SearchEngine.Search;

using LandmarkIT.Enterprise.Utilities.Logging;
using Nest;
using System.Threading.Tasks;
using System.Configuration;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Domain.Dropdowns;

namespace SmartTouch.CRM.SearchEngine.Indexing
{
    internal class ContactIndexer<T> : Indexer<T, int> where T : Contact
    {
        public override bool SetupIndex(int accountId)
        {
            string indexName = "contacts" + accountId;
            var client = ElasticClient(indexName);

            bool result = this.createIndex(indexName, client);
            if (result == false)
                throw new InvalidOperationException("Error occurred while creating index:" + indexName);
            return true;
        }

        public override int ReIndexAll(IEnumerable<T> documents)
        {
            IList<int> accounts = documents.Select(c => c.AccountID).Distinct().ToList();
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int indexedDocuments = 0;
            foreach (int accountId in accounts)
            {
                var count = reIndexContacts(documents.Where(c => c.AccountID == accountId), accountId);
                indexedDocuments = indexedDocuments + count;
                Console.WriteLine("Indexed " + count + " of account " + accountId + " at " + DateTime.Now.ToString());
            }
            sw.Stop();
            var timeelapsed = sw.Elapsed;
            Logger.Current.Informational("Time elapsed for reindexing contacts to elastic:" + timeelapsed);
            return indexedDocuments;
        }

        public override bool IndexExists()
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);
            return client.IndexExists(indexName).Exists;
        }

        public override int Index(IEnumerable<T> documents)
        {
            IList<int> accounts = documents.Select(c => c.AccountID).Distinct().ToList();
            int indexedDocuments = 0;

            foreach (int accountId in accounts)
                indexedDocuments = indexedDocuments + indexContacts(documents.Where(d => d.AccountID == accountId), accountId);

            return indexedDocuments;
        }

        public override int Index(T document)
        {
            string indexName = "contacts" + document.AccountID;
            var client = ElasticClient(indexName);

            int count = 0;
            if (client.IndexExists(indexName).Exists)
            {
                IList<Type> types = new List<Type>() { typeof(Person), typeof(Company) };
                client.Search<Contact>(c => c.Types(types).Query(q => q.Ids(new List<string>() { document.Id.ToString() })));
                
                IList<Contact> contacts = new List<Contact>() { document };
                if (document.GetType().Equals(typeof(Person)))
                {
                    var deleteResult = client.Bulk(b => b.DeleteMany(contacts, (bid, c) => bid.Type(typeof(Person)).Index(indexName)));
                    string dr = deleteResult.ConnectionStatus.ToString();
                    //Logger.Current.Verbose(dr);
                    var result = client.Bulk(b => b.IndexMany(contacts, (bid, c) => bid.Type(typeof(Person)).Index(indexName)
                        .VersionType(Elasticsearch.Net.VersionType.Internal)));
                    string qu = result.ConnectionStatus.ToString();
                    //Logger.Current.Verbose(qu);
                    count = result.Items != null ? result.Items.Count() : 0;
                    var refreshResult = client.Refresh(r => r.Index(indexName));
                    string qe = refreshResult.ConnectionStatus.ToString();
                    //Logger.Current.Verbose(qe);
                    if (contacts != null && contacts.Any()) Logger.Current.Verbose(string.Format("Indexed Contact : {0} - Created On - {1}", contacts.First().Id, contacts.First().CreatedOn.ToString()));
                }
                else if (document.GetType().Equals(typeof(Company)))
                {
                    var deleteResult = client.Bulk(b => b.DeleteMany(contacts, (bid, c) => bid.Type(typeof(Company)).Index(indexName)));
                    string dr = deleteResult.ConnectionStatus.ToString();
                    //Logger.Current.Verbose(dr);
                    var result = client.Bulk(b => b.IndexMany(contacts, (bid, c) => bid.Type(typeof(Company)).Index(indexName)
                        .VersionType(Elasticsearch.Net.VersionType.Internal)));
                    count = result.Items != null ? result.Items.Count() : 0;
                    var refreshResult = client.Refresh(r => r.Index(indexName));
                    string qe = refreshResult.ConnectionStatus.ToString();
                    //Logger.Current.Verbose(qe);
                }
            }
            Logger.Current.Informational("Total Indexed contacts:" + count);
            return count;
        }

        public override int Update(T document)
        {
            string indexName = "contacts" + document.AccountID;
            var client = ElasticClient(indexName);

            int count = 0;
            if (client.IndexExists(indexName).Exists)
            {
                if (document.GetType().Equals(typeof(Person)))
                {
                    var updateResult = client.Update<Contact>(u => u
                        .Doc(document)
                        .Id(document.Id)
                        .Type(typeof(Person))
                        .VersionType(Elasticsearch.Net.VersionType.Internal)
                        .Refresh(true));

                    if (updateResult.IsValid)
                        count = 1;
                    string qe = updateResult.ConnectionStatus.ToString();
                    //Logger.Current.Verbose(qe);

                    var percolateResponse = client.Percolate<Contact>(p => p.Document(document).Id(document.Id));
                    if (percolateResponse != null && percolateResponse.Matches.Any())
                        Logger.Current.Informational("No of records matched : " + percolateResponse.Matches.Count());
                }
                else if (document.GetType().Equals(typeof(Company)))
                {
                    var updateResult = client.Update<Contact>(u => u
                       .Doc(document)
                       .Id(document.Id)
                       .Type(typeof(Company))
                       .VersionType(Elasticsearch.Net.VersionType.Internal)
                       .Refresh(true));

                    if (updateResult.IsValid)
                        count = 1;
                    string qe = updateResult.ConnectionStatus.ToString();
                    //Logger.Current.Verbose(qe);

                    var percolateResponse = client.Percolate<Contact>(p => p.Document(document).Id(document.Id));
                    if (percolateResponse != null && percolateResponse.Matches.Any())
                        Logger.Current.Informational("No of records matched : " + percolateResponse.Matches.Count());
                }
            }
            Logger.Current.Informational("Total Updated contacts:" + count);
            return count;
        }

        public override int Remove(T document)
        {
            return removeContact(document.Id, document.AccountID);
        }

        public override int Remove(int id)
        {
            throw new NotImplementedException();
        }

        public override int Remove(int documentId, int accountId)
        {
            return removeContact(documentId, accountId);
        }

        int removeContact(int contactId, int accountId)
        {
            string indexName = "contacts" + accountId;
            var client = ElasticClient(indexName);
            int count = 0;

            if (client.IndexExists(i => i.Index(indexName)).Exists)
            {
                Contact contact = null;
                IList<Type> types = new List<Type>() { typeof(Person), typeof(Company) };
                var documentById = client.Search<Contact>(c => c.Types(types).Query(q => q.Ids(new List<string>() { contactId.ToString() })));
                if (documentById != null && documentById.Documents.IsAny())
                {
                    contact = documentById.Documents.FirstOrDefault();

                    IList<Contact> contacts = new List<Contact>() { contact };
                    if (contact.GetType().Equals(typeof(Person)))
                    {
                        var deleteResult = client.Bulk(b => b.DeleteMany(contacts, (bid, c) => bid.Type(typeof(Person)).Index(indexName)));
                        string dr = deleteResult.ConnectionStatus.ToString();
                        //Logger.Current.Verbose(dr);
                        count = deleteResult.Items.Count();
                    }
                    else if (contact.GetType().Equals(typeof(Company)))
                    {
                        var deleteResult = client.Bulk(b => b.DeleteMany(contacts, (bid, c) => bid.Type(typeof(Company)).Index(indexName)));
                        string dr = deleteResult.ConnectionStatus.ToString();
                        //Logger.Current.Verbose(dr);
                        count = deleteResult.Items.Count();
                    }
                }
            }
            Logger.Current.Informational("Total Removed Contacts:" + count);
            return count;
        }

        public override bool DeleteIndex()
        {
            string indexName = "contacts";
            var client = ElasticClient(indexName);
            var response = client.DeleteIndex(i => i.Index(indexName));
            return response.ConnectionStatus.Success;
        }

        bool createIndex(string indexName, ElasticClient client)
        {
            if (client.IndexExists(indexName).Exists)
                client.DeleteIndex(new DeleteIndexRequest(indexName));

            var customAnalyzer = new CustomAnalyzer
            {
                Filter = new List<string> { "standard", "lowercase", "stop" },
                Tokenizer = "uax_url_email"
            };

            var duplicateCheckAnalyzer = new CustomAnalyzer
            {
                Filter = new List<string> { "standard", "lowercase" },
                Tokenizer = "keyword"
            };
            var createResult = client.CreateIndex(indexName, index => index
                .Analysis(a => a
                .Analyzers(an => an
                    .Add("custom", customAnalyzer)
                    .Add("duplicateCheckAnalyzer", duplicateCheckAnalyzer)))
                .AddMapping<Contact>(pmd => MapContactCompletionFields<Contact>(pmd))
                .AddMapping<Person>(pmd => MapContactCompletionFields<Person>(pmd))
                .AddMapping<Company>(pmd => MapContactCompletionFields<Company>(pmd)));

            string qe = createResult.ConnectionStatus.ToString();
            Logger.Current.Verbose(qe);
            return createResult.ConnectionStatus.Success;
        }

        private int reIndexContacts(IEnumerable<Contact> documents, int accountId)
        {
            string indexName = "contacts" + accountId;
            var client = ElasticClient(indexName);

            int personCount = 0;
            int companyCount = 0;
            var persons = documents.Where(c => c.GetType().Equals(typeof(Person)));
            var companies = documents.Where(c => c.GetType().Equals(typeof(Company)));
            if (persons.IsAny())
            {
                Console.WriteLine("Indexing persons from " + persons.First().Id + " -- " + persons.Last().Id);
                var resultPerson = client.Bulk(b => b.IndexMany(persons, (bid, c) => bid.Type(typeof(Person)).Index(indexName)));
                personCount = resultPerson.Items != null ? resultPerson.Items.Count() : 0;

                //Logger.Current.Verbose(resultPerson.ConnectionStatus.ToString());
            }
            if (companies.IsAny())
            {
                Console.WriteLine("Indexing companies from " + companies.Last().Id + " -- " + companies.First().Id);
                var resultCompany = client.Bulk(b => b.IndexMany(companies, (bid, c) => bid.Type(typeof(Company)).Index(indexName)));
                companyCount = resultCompany.Items != null ? resultCompany.Items.Count() : 0;

                //Logger.Current.Verbose(resultCompany.ConnectionStatus.ToString());
            }
            int indexedCount = personCount + companyCount;
            Logger.Current.Verbose("Total Reindexed Contacts:" + indexedCount + ", Account ID :" + accountId);

            return indexedCount;
        }

        private PutMappingDescriptor<TContact> MapContactCompletionFields<TContact>(PutMappingDescriptor<TContact> pmd)
            where TContact : Contact
        {
            return pmd
                .Properties(props => props
                    .Completion(s => s
                          .Name(p => p.TitleAutoComplete)
                          .IndexAnalyzer("standard")
                          .SearchAnalyzer("standard")
                          .MaxInputLength(30)
                          .Payloads()
                          .PreservePositionIncrements()
                          .PreserveSeparators()
                     )
                     .Completion(s => s
                          .Name(p => p.ContactFullNameAutoComplete)
                          .IndexAnalyzer("standard")
                          .SearchAnalyzer("standard")
                          .MaxInputLength(75)
                          .Payloads()
                          .PreservePositionIncrements()
                          .PreserveSeparators()
                     )
                     .Completion(s => s
                          .Name(p => p.CompanyNameAutoComplete)
                          .IndexAnalyzer("standard")
                          .SearchAnalyzer("standard")
                          .MaxInputLength(50)
                          .Payloads()
                          .PreservePositionIncrements()
                          .PreserveSeparators()
                     )
                     .Completion(s => s
                          .Name(p => p.EmailAutoComplete)
                          .IndexAnalyzer("standard")
                          .SearchAnalyzer("standard")
                          .MaxInputLength(200)
                          .Payloads()
                          .PreservePositionIncrements()
                          .PreserveSeparators()
                     )
                      .Completion(s => s
                          .Name(p => p.PhoneNumberAutoComplete)
                          .IndexAnalyzer("standard")
                          .SearchAnalyzer("standard")
                          .MaxInputLength(200)
                          .Payloads()
                          .PreservePositionIncrements()
                          .PreserveSeparators()
                     )
                     .String(s => s.Name("firstName").Boost(1.0))
                     .String(s => s.Name("lastName").Boost(1.0))
                     .String(s => s.Name("title").Boost(1.0))

                     .String(s => s.Name("addresses.addressLine1").Index(FieldIndexOption.Analyzed).Store(true).IndexAnalyzer("duplicateCheckAnalyzer").SearchAnalyzer("duplicateCheckAnalyzer"))
                     .String(s => s.Name("addresses.addressLine2").Index(FieldIndexOption.Analyzed).Store(true).IndexAnalyzer("duplicateCheckAnalyzer").SearchAnalyzer("duplicateCheckAnalyzer"))
                     .String(s => s.Name("addresses.city").Index(FieldIndexOption.Analyzed).Store(true).IndexAnalyzer("duplicateCheckAnalyzer").SearchAnalyzer("duplicateCheckAnalyzer"))
                     .String(s => s.Name("addresses.zipCode").Index(FieldIndexOption.Analyzed).Store(true).IndexAnalyzer("duplicateCheckAnalyzer").SearchAnalyzer("duplicateCheckAnalyzer"))
                     //FieldIndexOption.Analyzed will split the string into tokens, but if we use duplicateCheckAnalyzer with keyword tokenizer it won't split the string.
                     .Date(s => s.Name("createdOn").Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd"))
                     .Date(s => s.Name("lastContacted").Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd"))
                     .Date(s => s.Name("lastUpdatedOn").Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd"))
                     .Date(s => s.Name("lastNoteDate").Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd"))

                     .String(s => s.Name("addresses.state.code").Index(FieldIndexOption.NotAnalyzed))
                     .String(s=>s.Name("noteSummary").Index(FieldIndexOption.NotAnalyzed))
                     .String(s => s.Name("emails.emailId").Index(FieldIndexOption.NotAnalyzed).Boost(1.0))
                     .NestedObject<ContactTourCommunityMap>(a => a
                          .Name("tourCommunity")
                          .Dynamic()
                          .IncludeInRoot()
                          .Properties(p => p
                                .Number(n => n.Name(na => na.CommunityID))
                                .Number(n => n.Name(na => na.TourType))
                                .Number(n => n.Name(na => na.TourID))
                                .Number(n => n.Name(na => na.ContactId))
                                .Date(n => n.Name(na => na.TourDate).Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd"))
                                .Number(n => n.Name(na => na.CreatedBy))))
                     .NestedObject<ContactActionMap>(a => a
                          .Name("contactActions")
                          .Dynamic()
                          .IncludeInRoot()
                          .Properties(p => p
                                .Date(n => n.Name(na => na.ActionDate).Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd"))
                                .Number(n => n.Name(na => na.ActionID))
                                .Number(n => n.Name(na => na.ActionType))
                                .Number(n => n.Name(na => na.ContactId))
                                .Date(n => n.Name(na => na.CreatedOn).Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd"))))
                      .NestedObject<ContactNoteMap>(a => a
                            .Name("contactNotes")
                            .Dynamic()
                            .IncludeInRoot()
                            .Properties(p => p
                            .Date(n => n.Name(na => na.CreatedOn).Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd"))
                            .Number(n => n.Name(na => na.NoteID))
                            .Number(n => n.Name(na => na.NoteCategory))
                            .Number(n => n.Name(na => na.ContactID))
                            .Number(n => n.Name(na => na.CreatedBy))
                            .String(n => n.Name(na => na.NoteDetails))))
                     .NestedObject<ContactCustomField>(no => no
                         .Name("customFields")
                         .Dynamic()
                         .IncludeInRoot()
                         .Properties(p => p
                            .String(s => s.Name(sn => sn.Value).Index(FieldIndexOption.NotAnalyzed))
                            .Date(d => d.Name(dn => dn.Value_Date).Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd").NullValue(new DateTime()))
                            .Number(n => n.Name(nam => nam.Value_Multiselect))
                            .Number(n => n.Name(nam => nam.Value_Multiselect_Count))
                            .Number(n => n.Name(nam => nam.Value_Number))
                            .Number(n => n.Name(nam => nam.FieldInputTypeId))
                            .Number(n => n.Name(nam => nam.CustomFieldId))
                            .Number(n => n.Name(nam => nam.ContactId))
                            .Number(n => n.Name(nam => nam.ContactCustomFieldMapId))))
                     .NestedObject<WebVisit>(w => w
                          .Name("webVisits")
                          .Dynamic()
                          .IncludeInRoot()
                          .Properties(p => p
                            .Number(s => s.Name(sn => sn.ContactID))
                            .Date(s => s.Name(sn => sn.VisitedOn).Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd").NullValue(new DateTime()))
                            .String(s => s.Name(sn => sn.PageVisited).Index(FieldIndexOption.NotAnalyzed))
                            .Number(s => s.Name(sn => sn.Duration))))
                    .NestedObject<DropdownValue>(w => w
                          .Name("leadSources")
                          .Dynamic()
                          .IncludeInRoot()
                          .Properties(p => p
                                .Date(s => s.Name(na => na.LastUpdatedDate).Format("yyyy-MM-dd'T'HH:mm:ss.SSS||yyyy-MM-dd'T'HH:mm:ss||MM/dd/yyyy||yyyy/MM/dd").NullValue(new DateTime()))
                                .Boolean(s => s.Name(na => na.IsPrimary))
                                .Number(s => s.Name(na => na.Id))))
                     .MultiField(m => m
                        .Name(n => n.FacebookLink)
                        .Fields(f => f
                            .String(s => s.Name(n => n.FacebookLink)
                                          .Index(FieldIndexOption.Analyzed))
                            .String(s => s.Name(n => n.FacebookLink.Suffix("Raw"))
                                          .Index(FieldIndexOption.Analyzed)
                                          .Store(true)
                                          .IndexAnalyzer("duplicateCheckAnalyzer")
                                          .SearchAnalyzer("duplicateCheckAnalyzer"))))
                    .MultiField(m => m
                        .Name(n => n.TwitterLink)
                        .Fields(f => f
                            .String(s => s.Name(n => n.TwitterLink)
                                          .Index(FieldIndexOption.Analyzed))
                            .String(s => s.Name(n => n.TwitterLink.Suffix("Raw"))
                                        .Index(FieldIndexOption.Analyzed)
                                        .Store(true)
                                        .IndexAnalyzer("duplicateCheckAnalyzer")
                                        .SearchAnalyzer("duplicateCheckAnalyzer"))))
                     .MultiField(m => m
                        .Name(n => n.LinkedInLink)
                        .Fields(f => f
                            .String(s => s.Name(n => n.LinkedInLink)
                                          .Index(FieldIndexOption.Analyzed))
                            .String(s => s.Name(n => n.LinkedInLink.Suffix("Raw"))
                                        .Index(FieldIndexOption.Analyzed)
                                        .Store(true)
                                        .IndexAnalyzer("duplicateCheckAnalyzer")
                                        .SearchAnalyzer("duplicateCheckAnalyzer"))))
                     .MultiField(m => m
                        .Name(n => n.GooglePlusLink)
                        .Fields(f => f
                            .String(s => s.Name(n => n.GooglePlusLink)
                                          .Index(FieldIndexOption.Analyzed))
                            .String(s => s.Name(n => n.GooglePlusLink.Suffix("Raw"))
                                        .Index(FieldIndexOption.Analyzed)
                                        .Store(true)
                                        .IndexAnalyzer("duplicateCheckAnalyzer")
                                        .SearchAnalyzer("duplicateCheckAnalyzer"))))
                     .MultiField(m => m
                        .Name(n => n.WebsiteLink)
                        .Fields(f => f
                            .String(s => s.Name(n => n.WebsiteLink)
                                          .Index(FieldIndexOption.Analyzed))
                            .String(s => s.Name(n => n.WebsiteLink.Suffix("Raw"))
                                        .Index(FieldIndexOption.Analyzed)
                                        .Store(true)
                                        .IndexAnalyzer("duplicateCheckAnalyzer")
                                        .SearchAnalyzer("duplicateCheckAnalyzer"))))
                     .MultiField(m => m
                        .Name(n => n.BlogLink)
                        .Fields(f => f
                            .String(s => s.Name(n => n.BlogLink)
                                          .Index(FieldIndexOption.Analyzed))
                            .String(s => s.Name(n => n.BlogLink.Suffix("Raw"))
                                        .Index(FieldIndexOption.Analyzed)
                                        .Store(true)
                                        .IndexAnalyzer("duplicateCheckAnalyzer")
                                        .SearchAnalyzer("duplicateCheckAnalyzer"))))
                     .MultiField(m => m
                        .Name(s => s.CompanyName)
                        .Fields(f => f
                             .String(s => s.Name(n => n.CompanyName)
                                 .Index(FieldIndexOption.Analyzed).Boost(1.0))
                            .String(s => s.Name(n => n
                                 .CompanyName.Suffix("companyNameFull"))
                                 .Index(FieldIndexOption.Analyzed)
                                 .Store(true)
                                 .IndexAnalyzer("duplicateCheckAnalyzer")
                                 .SearchAnalyzer("duplicateCheckAnalyzer"))))
                    .MultiField(m => m
                        .Name("firstName")
                        .Fields(f => f
                             .String(s => s.Name("firstName")
                                 .Index(FieldIndexOption.Analyzed))
                            .String(s => s.Name(n => n
                                 .CompanyName.Suffix("firstNameFull"))
                                 .Index(FieldIndexOption.Analyzed)
                                 .Store(true)
                                 .IndexAnalyzer("duplicateCheckAnalyzer")
                                 .SearchAnalyzer("duplicateCheckAnalyzer"))))
                    .MultiField(m => m
                        .Name("lastName")
                        .Fields(f => f
                             .String(s => s.Name("lastName")
                                 .Index(FieldIndexOption.Analyzed))
                            .String(s => s.Name(n => n
                                 .CompanyName.Suffix("lastNameFull"))
                                 .Index(FieldIndexOption.Analyzed)
                                 .Store(true)
                                 .IndexAnalyzer("duplicateCheckAnalyzer")
                                 .SearchAnalyzer("duplicateCheckAnalyzer"))))
                    .MultiField(m => m.
                        Name("title")
                        .Fields(f => f
                        .String(s => s.Name("title")
                            .Index(FieldIndexOption.Analyzed))
                        .String(s => s.Name(n => n
                            .CompanyName.Suffix("titleFull"))
                            .Index(FieldIndexOption.Analyzed)
                                 .Store(true)
                                 .IndexAnalyzer("duplicateCheckAnalyzer")
                                 .SearchAnalyzer("duplicateCheckAnalyzer"))))
                    .Date(d => d
                        .Name("_timestamp")
                        .Store(true))
                     )
                .TimestampField(t => t
                    .Enabled(true));
        }

        private int indexContacts(IEnumerable<T> documents, int accountId)
        {
            string indexName = "contacts" + accountId;
            var client = ElasticClient(indexName);
            if (!client.IndexExists(indexName).Exists)
            {
                var createResult = client.CreateIndex(indexName, index => index
                    .AddMapping<Contact>(tmd => MapContactCompletionFields(tmd))
                    .AddMapping<Person>(tmd => MapContactCompletionFields(tmd))
                    .AddMapping<Company>(tmd => MapContactCompletionFields(tmd)));

                string qe = createResult.ConnectionStatus.ToString();
                Logger.Current.Verbose(qe);
            }
            int personCount = 0;
            int companyCount = 0;

            var persons = documents.Where(c => c.GetType().Equals(typeof(Person)));
            if (persons.IsAny())
            {
                var task_resultPerson = client.BulkAsync(b => b.IndexMany(persons, (bid, c) => bid.Type(typeof(Person)).Index(indexName)));
                Task.WaitAll(task_resultPerson);
                personCount = task_resultPerson.Result.Items != null ? task_resultPerson.Result.Items.Count() : 0;
                //Logger.Current.Verbose(task_resultPerson.Result.ConnectionStatus.ToString());
            }
            var companies = documents.Where(c => c.GetType().Equals(typeof(Company)));
            if (companies.IsAny())
            {
                var task_resultPerson = client.BulkAsync(b => b.IndexMany(companies, (bid, c) => bid.Type(typeof(Company)).Index(indexName)));
                Task.WaitAll(task_resultPerson);
                companyCount = task_resultPerson.Result.Items != null ? task_resultPerson.Result.Items.Count() : 0;
                //Logger.Current.Verbose(task_resultPerson.Result.ConnectionStatus.ToString());
            }

            int indexedCount = personCount + companyCount;
            Logger.Current.Verbose("Total Reindexed Contacts:" + indexedCount);
            return indexedCount;
        }

        QueryDescriptor<T> getQueryDescriptor()
        {
            return null;
        }

    }
}
