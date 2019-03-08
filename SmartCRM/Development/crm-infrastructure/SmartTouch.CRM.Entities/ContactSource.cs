using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartTouch.CRM.Entities
{
    public enum ContactSource:byte
    {
        [Display(Name="Lead Adapter")]
        LeadAdapter = 1,
        [Display(Name = "Imports")]
        Import =2,
        [Display(Name = "Forms")]
        Forms = 3,
        [Display(Name = "Manual")]
        Manual = 4,
        [Display(Name = "Smarttouch API")]
        API = 5,
        [Display(Name = "Email Sync")]
        EmailSync = 6
    }
}
