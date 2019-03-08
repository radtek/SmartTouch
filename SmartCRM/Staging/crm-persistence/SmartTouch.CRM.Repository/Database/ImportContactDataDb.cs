using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SmartTouch.CRM.Repository.Database
{
    public class ImportContactData
    {
        [Key]
        public Int32 ImportContactDataID { get; set; }
       public string FirstName{get; set;}
       public string LastName{get; set;}
       public string  CompanyName{get; set;}
       public string Title{get; set;}
       public string LeadSource{get; set;} 
       public string LifecycleStage{get; set;} 
       public string PartnerType{get; set;} 
       public bool? DoNotEmail{get; set;} 
       public string HomePhone{get; set;}
       public string MobilePhone{get; set;}
      public string WorkPhone{get; set;} 
      public int? AccountID{get; set;}
      public string PrimaryEmail{get; set;} 
      public string SecondaryEmails{get; set;} 
      public string FacebookUrl{get; set;} 
      public string TwitterUrl{get; set;} 
      public string GooglePlusUrl{get; set;} 
      public string LinkedInUrl{get; set;} 
      public string BlogUrl{get; set;} 
      public string WebSiteUrl{get; set;} 
      public string AddressLine1{get; set;} 
      public string AddressLine2{get; set;} 
      public string City{get; set;} 
      public string State{get; set;} 
      public string Country{get; set;}
      public string ZipCode{get; set;} 
      public string CustomFieldsData{get; set;}

      public int? ContactID { get; set; }
      public byte? ContactStatusID { get; set; }
      public Guid? ReferenceID { get; set; }
      public byte? ContactTypeID { get; set; }
      public int? OwnerID { get; set; }
      public int? JobID { get; set; } 
      public Int16? LeadSourceID { get; set; }
      public string PhoneData { get; set; }
      public bool? EmailExists { get; set; }
      public bool? IsBuilderNumberPass { get; set; }
      public string LeadAdapterSubmittedData { get; set; }
      public string LeadAdapterRowData { get; set; }
      public bool ValidEmail { get; set; }
      public string OrginalRefId { get; set; }
      public bool IsDuplicate { get; set; }
      public bool? IsCommunityNumberPass { get; set; }
    }
}
