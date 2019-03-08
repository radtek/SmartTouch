using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum DropdownValueTypes : short
    {
        HomeAddress = 2,
        MailingAddress = 5,
        WorkAddress = 6,
        PhysicalAddress = 7,
        BillingAddress = 8,
        MobilePhone = 9,
        WorkPhone = 10,
        Homephone = 11,
        FaxPhone = 39,
        Subscriber = 12,
        Lead = 13,
        Prospect = 14,
        Customer = 15,
        Partner = 16,
        Realtor = 17,
        Vendor = 18,
        Supplier = 19,
        Facebook = 20,
        Twitter = 21,
        GooglePlus = 22,
        Imports = 40,
        Interested = 23,
        Offer = 24,
        Contract = 25,
        Closed = 26,
        Lost = 27,
        Community1 = 28,
        Community2 = 29,
        Community3 = 30,
        First = 31,
        Be_Back = 32,
        Agent = 33,
        Spouse = 34,        
        Lawyer = 36,
        Forms = 37,
        Leadadapters = 38,
        Custom = 3,
        PhoneCall = 46,
        Email = 47,
        Appointment = 48,
        [Display(Name = "Contract")]
        ContractNote =50,
        EmailExchange =51,
        LVM = 52,
        PhoneConversation = 53,
        ProposalOut=54,
        SentCollateral=55,
        ThankYouNote=56,
        ActionDetails=57,
        TourDetails=58
    }
}
