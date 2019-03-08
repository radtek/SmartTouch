using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using System.Threading;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Notification
    {
        /// <summary>
        /// Id of the notification to manage the client
        /// </summary>

        /// <summary>
        /// NotificationId is for database persistance
        /// </summary>
        public int NotificationID { get; set; }

        public NotificationSource Source { get; set; }

        public int? EntityId { get; set; }

        /// <summary>
        /// The headline for the notification
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// A little bit more information about the notification
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// The time when the notification starts
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// How many minutes does the meeting last
        /// </summary>
        public int Duration { get; set; }

        public int? UserID { get; set; }

        public byte ModuleID { get; set; }

        public NotificationStatus Status { get; set; }

        public string DownloadFile { get; set; }

        public List<ContactNotificationEntry> ContactEntries { get; set; }

        public List<OpportunityNotificationEntry> OpportunityEntries { get; set; }

        public List<EmailClients> EmailClients { get; set; }


        public string TimeInText
        {
            get
            {
                int differenceInSeconds = (int)(DateTime.Now.ToUniversalTime() - this.Time).TotalSeconds;
                int differenceInMinutes = (int)(differenceInSeconds / 60);
                int differenceInHours = (int)(differenceInMinutes / 60);
                if (differenceInSeconds < 60)
                    return "Few seconds ago";
                else if (differenceInMinutes < 60)
                    return differenceInMinutes + " minutes ago";
                else if (differenceInHours < 24)
                    return differenceInHours + " hours ago";
                else
                    return this.Time.ToString();
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;                
                hash = hash * 23 + this.ModuleID;
                if (this.EntityId.HasValue)
                    hash = hash * 23 + this.EntityId.Value;
                if(this.NotificationID != 0)
                    hash = hash * 23 + this.NotificationID;

                return hash;
            }
        }
    }

    public class ContactNotificationEntry
    {
        public int ContactID { get; set; }
        public int ContactType { get; set; }
        public string FullName { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class OpportunityNotificationEntry
    {
        public int OpportunityId { get; set; }
        public string OpportunityName { get; set; }
    }

    public class EmailClients
    {
        public string Client { get; set; }
        public string Value { get; set; }
        public string LitmusId { get; set; }
    }
}
