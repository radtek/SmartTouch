using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class SentMailQueueDb
    {
        [Key]
        public int SentMailQueueID { get; set; }
        public Guid TokenGuid { get; set; }
        public Guid RequestGuid { get; set; }
        public string From { get; set; }
        public MailPriority PriorityID { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public DateTime QueueTime { get; set; }
        public CommunicationStatus StatusID { get; set; }
        public string ServiceResponse { get; set; }
        public bool GetProcessedByClassic { get; set; }
    }
}
