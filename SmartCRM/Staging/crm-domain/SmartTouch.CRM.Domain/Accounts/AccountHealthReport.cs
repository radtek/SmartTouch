using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Accounts
{
    public class AccountHealthReport
    {
        public List<HealthFormReport> Forms { get; set; }
        public List<HealthCampaignReport> Campaigns { get; set; }       
        public List<HealthEmailsReport> Emails { get; set; }
        public List<HealthImportsReport> HealthImportsReport { get; set; }
        public List<HealthLeadAdaptersReport> HealthLeadAdaptersReport { get; set; }        
        public List<HealthWorkflowReport> HealthWorkflowReport { get; set; }
        public List<CampaignSent> CampaignSent { get; set; }
        public List<FailedImports> FailedImports { get; set; }
        public List<SucceededImports> SucceededImports { get; set; }
        public List<InProgressImport> InProgressImport { get; set; }
        public List<FailedLeadAdapter> FailedLeadAdapter { get; set; }
        public List<SucceededLeadAdapters> SucceededLeadAdapters { get; set; }
        public List<ContactLeadScore> ContactLeadScore { get; set; }
    }

    public class HealthFormReport
    {
        public string Data { get; set; }        
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }
    }

    public class HealthCampaignReport
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public int Failed { get; set; }
        public int Sending { get; set; }
    }

    public class HealthImportsReport
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }
        public int Count { get; set; }
    }

    public class HealthLeadAdaptersReport
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }
    }

    public class HealthEmailsReport
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
    }

    public class HealthWorkflowReport
    {                
        public string AccountName { get; set; }
        public int AccountID { get; set; }
        public int? ContactsCount { get; set; }
    }  
    
    
    public class CommonHealthReport
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }
        public int Count { get; set; }
    }

    public class CampaignSent
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }        
    }

    public class FailedImports
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }        
    }

    public class SucceededImports
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }        
    }

    public class InProgressImport
    {
        public string Date { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }        
    }

    public class FailedLeadAdapter
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }       
    }

    public class SucceededLeadAdapters
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }        
    }

    public class ContactLeadScore
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }        
    }
}
