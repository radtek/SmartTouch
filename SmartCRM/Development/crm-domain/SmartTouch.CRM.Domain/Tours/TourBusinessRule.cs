using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Tours
{
    public static class TourBusinessRule
    {
        public static readonly BusinessRule DetailsMaxLengthReached = new BusinessRule("[|Enter 1000 or less characters.|]");
        public static readonly BusinessRule ContactsRequired = new BusinessRule("[|Include at least one contact.|]");
        public static readonly BusinessRule CommunityRequired = new BusinessRule("[|Community is required.|]");
        public static readonly BusinessRule TourTypeRequired = new BusinessRule("[|Tour type is required.|]");
        public static readonly BusinessRule TourDateNotValid = new BusinessRule("[|Please select a valid Tour date|]");
        public static readonly BusinessRule ReminderNotApplicable = new BusinessRule("[|Reminder cannot be set for past or current DateTime.|]");
        public static readonly BusinessRule ReminderDateNotValid = new BusinessRule("[|Please select a valid reminder date|]");
    }
}
