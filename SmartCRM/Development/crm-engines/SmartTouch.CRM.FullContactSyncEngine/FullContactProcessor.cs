using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;


namespace SmartTouch.CRM.FullContactSyncEngine
{
    public class FullContactProcessor
    {
        int UserId = 1;
        int AccountId = 0;
        int Limit = 500;
        int PageNumber = 0;
        private static double notificationInterval = 60000d;
        private readonly TimeSpan _fullcontactInterval = TimeSpan.FromMilliseconds(notificationInterval);
        private static bool isRunning = default(bool);
        readonly IContactService contactService;

        public FullContactProcessor()
        {
            this.contactService = IoC.Container.GetInstance<IContactService>();
            var UserIdString = ConfigurationManager.AppSettings["USERID"].ToString();
            int.TryParse(UserIdString, out UserId);
            var limitString = ConfigurationManager.AppSettings["LIMIT"].ToString();
            int.TryParse(limitString, out Limit);
        }

        public void RefreshData()
        {
            try
            {
                var accountsConfigValue = ConfigurationManager.AppSettings["ACCOUNTS"].ToString();
                var accounts = accountsConfigValue.Split(',');
                foreach (var account in accounts)
                {
                    Logger.Current.Informational("Request received for refreshing contacts data for accountId : " + AccountId);
                    int.TryParse(account, out AccountId);
                    if (AccountId != 0)
                    {
                        DataRefresh(null);
                        AccountId = 0;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while refreshing data :" + e.StackTrace.ToString());
                Logger.Current.Error("An error occured while refreshing data :", e);
            }

        }

        public void DataRefresh(object state)
        {
            if (isRunning) return;
            isRunning = true;

            try
            {
                int refreshedContacts = 0;
                if (AccountId != 0 && Limit != 0)
                {
                    contactService.ProcessFullContactSync(new ProcessFullContactRequest() { AccountId = AccountId, Limit = Limit, RequestedBy = UserId });
                }
                Console.WriteLine("No of contacts refreshed are :" + refreshedContacts + ", Account Id: " + AccountId);
                isRunning = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured inside DataRefresh : " + ex.StackTrace.ToString());
                Logger.Current.Error("An error occured inside DataRefresh : ", ex);
            }
        }
    }
}
