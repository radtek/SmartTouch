using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.SearchEngine.Indexing;

namespace SmartTouch.CRM.Automation.Core
{
   public class MessageProcessor
    {
       ISearchService<Contact> searchService;
       IIndexingService indexingService;
       IContactService contactService;

       public MessageProcessor(IContactService contactService, IIndexingService indexingService, ISearchService<Contact> searchService)
       {
           this.contactService = contactService;
           this.searchService = searchService;
           this.indexingService = indexingService;
       }
    }
}
