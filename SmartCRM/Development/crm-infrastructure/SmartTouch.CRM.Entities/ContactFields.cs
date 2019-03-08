using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum ContactFields : int
    {
        FirstNameField = 1,
        LastNameField = 2,
        CompanyNameField = 3,
        MobilePhoneField = 4,
        HomePhoneField = 5,
        WorkPhoneField = 6,
        PrimaryEmail = 7,
        TitleField = 8,
        FacebookUrl = 9,
        TwitterUrl = 10,
        LinkedInUrl = 11,
        GooglePlusUrl = 12,
        WebsiteUrl = 13,
        BlogUrl = 14,
        AddressLine1Field = 15,
        AddressLine2Field = 16,
        CityField = 17,
        StateField = 18,
        ZipCodeField = 19,
        CountryField = 20,
        PartnerTypeField = 21,
        LifecycleStageField = 22,
        DonotEmail = 23,
        LeadSource = 24,
        Owner = 25,
        LeadScore = 26,
        CreatedBy = 27,
        CreatedOn = 28,
        LastTouched = 29,
        FirstName_NotAnalyzed = 30,
        LastName_NotAnalyzed = 31,
        CompanyName_NotAnalyzed = 32,
        LastUpdateOn = 33,
        StateCodeField = 34,
        CountryCode = 35,
        ContactId = 36,
        DropdownField = 37,   //Need to re-verify later
        PrimaryEmailStatus = 38,    //Advanced Search
        IsPrimaryEmail = 39,        // Advanced Search
        SecondaryEmail = 40,    
        LastTouchedThrough = 41,
        Community = 42,
        CompanyId = 43,    //Advanced Search(In case of people having companies)
        FirstSourceType = 44,
        WebPage = 45,
        WebPageDuration = 46,   
        ContactTag = 47,
        FormName = 48,
        FormsubmittedOn = 49,
        LeadSourceDate = 50,
        FirstLeadSource = 51,
        FirstLeadSourceDate = 52,
        AllLeadSources = 53,     //Don't assign this id to any field in fields table
        NoteSummary = 54,
        LastNoteDate = 55,
        TourType = 56,
        TourDate = 57,
        TourCreator = 58,
        EmailStatus = 59,
        TourAssignedUsers = 60,
        LeadAdapter = 61,
        CustomFields = 62,
        ActionCreatedDate = 63,
        ActionType = 64,
        ActionDate = 65,
        ActionStatus = 66,
        ActionAssignedTo = 67,
        ContactEmailID = 68, //Used for NeverBounce
        LastNote = 69,
        IncludeInReports = 70,
        NoteCategory=71,
        LastNoteCategory=72,
        IsActive = 73       //Used for campaign recipients
        //FormURLField = 74
    }
}
