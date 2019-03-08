using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowActionTypesDb
    {
        [Key]
        public WorkflowActionType WorkflowActionTypeID { get; set; }
        public string WorkflowActionName { get; set; }
        public bool IsLinkAction { get; set; }
    }
}
