using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Forms;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.Domain.Contacts;
using System.Configuration;
using System.Threading;
using SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList;
using SmartTouch.CRM.ApplicationServices.Messaging;

namespace SmartTouch.CRM.SearchEngine.Client
{
    public class ElasticDataProcessor
    {
        readonly IContactService contactService;
        readonly ISuppressionListService suppressionListService;
        readonly ICampaignService campaignService;
        readonly IOpportunitiesService opportunityService;
        readonly IFormService formService;
        readonly ITagService tagService;
        readonly IActionService actionService;
        readonly ITourService tourService;
        readonly IIndexingService indexingService;
        readonly IWebAnalyticsProviderService webAnalyticsService;
        readonly IAdvancedSearchService advancedSearchService;
        readonly IContactRepository contactRepository;
        public ElasticDataProcessor()
        {
            this.contactService = IoC.Container.GetInstance<IContactService>();
            this.campaignService = IoC.Container.GetInstance<ICampaignService>();
            this.opportunityService = IoC.Container.GetInstance<IOpportunitiesService>();
            this.formService = IoC.Container.GetInstance<IFormService>();
            this.tagService = IoC.Container.GetInstance<ITagService>();
            this.indexingService = IoC.Container.GetInstance<IIndexingService>();
            this.actionService = IoC.Container.GetInstance<IActionService>();
            this.tourService = IoC.Container.GetInstance<ITourService>();
            this.webAnalyticsService = IoC.Container.GetInstance<IWebAnalyticsProviderService>();
            this.advancedSearchService = IoC.Container.GetInstance<IAdvancedSearchService>();
            this.contactRepository = IoC.Container.GetInstance<IContactRepository>();
            this.suppressionListService = IoC.Container.GetInstance<ISuppressionListService>();
        }

        public void ReIndexAllEntities()
        {
            var modules = System.Configuration.ConfigurationManager.AppSettings["MODULES"].ToString();
            
            Console.WriteLine("Initiating re-indexing of entites (" + modules + ")");
            Console.WriteLine("Are you sure you want to re-index above entities? y/n");

            ConsoleKeyInfo info = Console.ReadKey();
            
            if (info.Key == ConsoleKey.N)
                return;
            
            if (modules.Contains("Contacts"))
                IndexContacts();
            
            if (modules.Contains("Tags"))
                IndexTags();
            
            if (modules.Contains("Campaigns"))
                IndexCampaigns();
            
            if (modules.Contains("Opportunities"))
                IndexOpportunities();
            
            if (modules.Contains("Forms"))
                IndexForms();
            
            if (modules.Contains("Formsubmissions"))
                IndexFormSubmissions();
            
            if (modules.Contains("Actions"))
                IndexActions();
            
            if (modules.Contains("Tours"))
                IndexTours();
            
            if (modules.Contains("WebVisits"))
                IndexWebVisits();
            
            if (modules.Contains("CampaignRecipients"))
                IndexCampaignRecipients();
            
            if (modules.Contains("SuppressionEmails"))
                IndexSuppressionEmails(1);
            
            if (modules.Contains("SuppressedDomains"))
                IndexSuppressionEmails(2);

            }

        private void IndexContacts()
        {
            try
            {
                List<int> accounts = GetAccounts();
                Action<int> ReIndex = (id) =>
                {
                    Console.WriteLine("Indexing contacts now..." + DateTime.Now.ToString());
                    var contactsResponse = contactService.ReIndexContacts(new ReIndexContactsRequest()
                    {
                        AccountId = id
                    });
                    Console.WriteLine("Indexing saved-searches now..." + DateTime.Now.ToString());
                    var savedSearchResponse = advancedSearchService.IndexSavedSearches(id);
                    Console.WriteLine("Total contacts indexed : " + contactsResponse.IndexedContacts + " Time:" + DateTime.Now.ToString());
                    Console.WriteLine("Total saved-searches indexed : " + savedSearchResponse + " Time:" + DateTime.Now.ToString());
                };

                if (accounts != null && accounts.Any())
                {
                    foreach (var account in accounts)
                    {
                        ReIndex(account);
                    }
                }
                else
                {
                    //reindex all contacts
                    ReIndex(0);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        private void IndexForms()
        {
            try
            {
                Console.WriteLine("Indexing forms now..." + DateTime.Now.ToString());
                indexingService.SetupStaticIndice<Form>();
                var formsResponse = formService.ReIndexForms(new ApplicationServices.Messaging.ReIndexDocumentRequest());
                Console.WriteLine("Total forms indexed : " + formsResponse.Documents + " Time:" + DateTime.Now.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void IndexFormSubmissions()
        {
            try
            {
                Console.WriteLine("Indexing formsubmissions now..." + DateTime.Now.ToString());
                indexingService.SetupStaticIndice<FormSubmission>();
                var formSubmissionResponse = formService.ReIndexFormSubmissions(new ApplicationServices.Messaging.ReIndexDocumentRequest());
                Console.WriteLine("Total form submissions indexed : " + formSubmissionResponse.Documents + " Time:" + DateTime.Now.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void IndexWebVisits()
        {
            try
            {
                var accountsConfigKey = System.Configuration.ConfigurationManager.AppSettings["ACCOUNTS"]
                                .ToString();
                var accounts = new List<int>();
                if (accountsConfigKey.Length > 0)
                    accounts = accountsConfigKey
                                    .Split(',')
                                    .Select(int.Parse).ToList();

                indexingService.SetupStaticIndice<WebVisit>();
                Action<int> ReIndex = (id) =>
                {
                    Console.WriteLine("Indexing webVisits now..." + DateTime.Now.ToString());
                    var webVisitsResponse = webAnalyticsService.ReIndexWebVisits(new ReIndexWebVisitsRequest()
                    {
                        AccountId = id
                    });
                    Console.WriteLine("Total Web visits indexed : " + webVisitsResponse.IndexedWebVisits + " Time:" + DateTime.Now.ToString());
                };

                if (accounts != null && accounts.Any())
                {
                    foreach (var account in accounts)
                    {
                        ReIndex(account);
                    }
                }
                else
                {
                    //reindex all contacts
                    ReIndex(0);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void IndexActions()
        {
            try
            {
                Console.WriteLine("Indexing actions now..." + DateTime.Now.ToString());
                indexingService.SetupStaticIndice<DA.Action>();
                var actionResponse = actionService.ReIndexActions(new ApplicationServices.Messaging.ReIndexDocumentRequest());
                Console.WriteLine("Total actions indexed : " + actionResponse.Documents + " Time:" + DateTime.Now.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void IndexSuppressionEmails(byte EmailsOrDomains)
        {
            try
            {
                Console.WriteLine("Indexing suppressed {0} now..." + DateTime.Now.ToString(), EmailsOrDomains == 1 ? "emails" : "domains");
                int batchCount = int.Parse(ConfigurationManager.AppSettings["CHUNK_SIZE"]);
                List<int> accounts = GetAccounts();
                Action<int> ReIndex = (id) =>
                {
                    Console.WriteLine("Indexing suppressed emails for accountid : " + id);
                    var suppressionResponse = suppressionListService.ReIndexSuppressionList(new ReIndexSuppressionListRequest()
                    {
                        IndexType = EmailsOrDomains,
                        AccountId = id,
                        SuppressionListBatchCount = batchCount
                    });
                    Console.WriteLine("Total suppressed {0} indexed : " + suppressionResponse.IndexedListCount + " Time:" + DateTime.Now.ToString(), EmailsOrDomains == 1 ? "emails" : "domains");
                };

                if (accounts != null && accounts.Any())
                {
                    foreach (var account in accounts)
                        ReIndex(account);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void IndexTours()
        {
            try
            {
                Console.WriteLine("Indexing Tours now..." + DateTime.Now.ToString());
                indexingService.SetupStaticIndice<Tour>();
                var tourResponse = tourService.ReIndexTours(new ApplicationServices.Messaging.ReIndexDocumentRequest());
                Console.WriteLine("Total tours indexed : " + tourResponse.Documents + " Time:" + DateTime.Now.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void IndexOpportunities()
        {
            try
            {
                Console.WriteLine("Indexing opportunities now..." + DateTime.Now.ToString());
                indexingService.SetupStaticIndice<Opportunity>();
                var opportunitiesResponse = opportunityService.ReIndexOpportunities(new ApplicationServices.Messaging.ReIndexDocumentRequest());
                Console.WriteLine("Total opportunities indexed : " + opportunitiesResponse.Documents + " Time" + DateTime.Now.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        private void IndexCampaigns()
        {
            try
            {
                List<int> accounts = GetAccounts();
                Action<int> ReIndex = (id) =>
                {
                    Console.WriteLine("Indexing campaigns now..." + DateTime.Now.ToString());
                    var campaignsResponse = campaignService.ReIndexCampaigns(new ReIndexDocumentRequest()
                    {
                        AccountId = id
                    });
                    Console.WriteLine("Total campaigns indexed : " + campaignsResponse.Documents + " Time:" + DateTime.Now.ToString());
                };

                if (accounts != null && accounts.Any())
                {
                    foreach (var account in accounts)
                    {
                        ReIndex(account);
                    }
                }
                else
                {
                    //reindex all contacts
                    ReIndex(0);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        private void IndexTags()
        {
            try
            {
                var accountsstring = System.Configuration.ConfigurationManager.AppSettings["ACCOUNTS"]
                               .ToString();
                var accounts = new List<int>();
                if (accountsstring.Length > 0)
                    accounts = accountsstring
                                    .Split(',')
                                    .Select(int.Parse).ToList();
                Console.WriteLine("Indexing tags now... " + DateTime.Now.ToString());
                

                Action<int> ReIndex = (id) =>
                {
                    
                    var tagsResponse = tagService.ReIndexTags(new ReIndexTagsRequest()
                        {
                            AccountId = id
                        });
                    Console.WriteLine("Total tags indexed : " + tagsResponse.IndexedTags + " Time: " + DateTime.Now.ToString());
                };

                if (accounts != null &&  accounts.Any())
                {
                    foreach (var account in accounts)
                    {
                        ReIndex(account);
                    }
                }
                else
                {
                    //reindex all tags
                    ReIndex(0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        private void DeleteAllIndices(string index)
        {
        }

        private void IndexCampaignRecipients()
        {
            var campaignstring = System.Configuration.ConfigurationManager.AppSettings["CAMPAIGN_RECIPIENTS"];
            var campaigns = new List<int>();
            if (campaignstring.Length > 0)
                campaigns = campaignstring
                                .Split(',')
                                .Select(int.Parse).ToList();
            Func<IEnumerable<int>, int, IEnumerable<IEnumerable<int>>> Chunk = (cts, chunkSize) =>
            {
                return cts
                    .Select((v, i) => new { v, groupIndex = i / chunkSize })
                    .GroupBy(x => x.groupIndex)
                    .Select(g => g.Select(x => x.v));
            };

            campaigns.ForEach(c =>
            {
                Console.WriteLine("Reindexing Campaign :" + c);
                var recipientsInfo = campaignService.GetCampaignRecipientsInfo(new ApplicationServices.Messaging.Campaigns.GetCampaignRecipientsRequest()
                {
                    CampaignId = c
                });
                Console.WriteLine("Total recipients count:" + recipientsInfo.RecipientsInfo.Count);
                var contacts = recipientsInfo.RecipientsInfo.Select(ct => ct.Key);

                var contacts_chunk = Chunk(contacts, int.Parse(ConfigurationManager.AppSettings["CHUNK_SIZE"]));
                Console.WriteLine("Chunks :" + contacts_chunk.Count());


                int index = 0;
                contacts_chunk.ToList().ForEach(documentIds =>
                {
                    index = index + 1;
                    var documents = contactRepository.FindAll(documentIds.ToList());
                    Console.WriteLine("Index started, iteration " + index);
                    try
                    {
                        indexingService.ReIndexAll(documents);
                        Thread.Sleep(3000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        Console.ReadLine();
                    }

                });
            });
        }

        private List<int> GetAccounts()
        {
            var accountsstring = System.Configuration.ConfigurationManager.AppSettings["ACCOUNTS"]
                                .ToString();
            var accounts = new List<int>();
            if (accountsstring.Length > 0)
                accounts = accountsstring
                                .Split(',')
                                .Select(int.Parse).ToList();
            return accounts;
        }
      }
}
