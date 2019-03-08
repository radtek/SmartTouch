using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum WorkflowStatus : short
    {
        [Display(Name="Active")]
        Active = 401,
        [Display(Name = "Draft")]
        Draft = 402,
        [Display(Name = "Paused")]
        Paused = 403,
        [Display(Name = "InActive")]
        InActive = 404,
        [Display(Name = "Archive")]
        Archive = 405
    }
}
