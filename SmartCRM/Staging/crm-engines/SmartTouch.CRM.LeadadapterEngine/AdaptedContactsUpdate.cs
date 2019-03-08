using LandmarkIT.Enterprise.Utilities.Logging;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.SearchEngine.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.LeadAdapterEngine
{
    public class AdaptedContactsUpdate :IJob
    {
        static DateTime lastModifiedDate = DateTime.MinValue;
        private static bool isRunning = default(bool);

        public void Execute(IJobExecutionContext context)
        {
            Trigger(null);          
        }
        public static void Trigger(Object stateInfo)
        {
            try
            {
                if (isRunning) return;
                isRunning = true;

                lastModifiedDate = (lastModifiedDate == DateTime.MinValue) ? DateTime.Now.ToUniversalTime() : lastModifiedDate;
                var accountService = IoC.Container.GetInstance<IAccountService>();
                var indexingService = IoC.Container.GetInstance<IIndexingService>();
                var contactRepository = IoC.Container.GetInstance<IContactRepository>();
                var tagService = IoC.Container.GetInstance<ITagService>();
                GetImportedContactsResponse response = accountService.GetImportedContacts(new GetImportedContactsRequest() { LastModifiedOn = lastModifiedDate });
                IEnumerable<Contact> Contacts = contactRepository.FindAll(response.ContactIds);
                var iterations = Math.Ceiling(Contacts.Count() / 2000m);

                for (var i = 0; i < iterations; i++)
                {
                    var contacts = Contacts.Skip(i * 2000).Take(2000);
                    indexingService.IndexContacts(contacts);
                }
              
                tagService.addToTopic(response.TagIds, response.ContactIds, Contacts.Select(k =>(int)k.OwnerId).FirstOrDefault(), Contacts.Select(k => k.AccountID).FirstOrDefault());
               // lastModifiedDate = DateTime.MinValue;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while indexing the contacts : " + ex);
            }
            finally
            {
                isRunning = false;
               // lastModifiedDate = DateTime.MinValue;
            }
        }
    }
}
