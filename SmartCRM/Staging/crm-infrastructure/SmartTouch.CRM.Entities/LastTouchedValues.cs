using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum LastTouchedValues : byte
    {
        [Display(Name = "Campaign")]
        Campaign = 4,
        [Display(Name = "Text")]
        SendText = 26,
        [Display(Name = "Email")]
        SendMail = 25,
        [Display(Name = "Phone Call")]
        PhoneCall = 46,
        [Display(Name = "Email")]
        Email = 47,
        [Display(Name = "Appointment")]
        Appointment = 48,
        [Display(Name = "Action-Other")]
        ActionOther = 3,
        [Display(Name = "Note")]
        Note = 6,
        [Display(Name = "Tour")]
        Tour = 7
    }
}
