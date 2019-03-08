using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CampaignContactViewModel
    {
        public int ContactId { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public Email Email { get; set; }
        public bool? DoNotEmail { get; set; }
        public ContactType ContactType { get; set; }

    }
}
