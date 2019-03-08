using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    /// <summary>
    /// Dropdown value
    /// </summary>
    public class DropdownValueViewModel
    {
        public Int16 SortID { get; set; }
        public string DropdownValue { get; set; }
        public Int16 DropdownValueID { get; set; }
        public int? AccountID { get; set; }
        public byte DropdownID { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public Int16 DropdownValueTypeID { get; set; }
        public int OpportunityGroupID { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsNewField { get; set; }
    }
}
