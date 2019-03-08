using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    /// <summary>
    /// Contact Data VM
    /// </summary>
    public class ContactDataViewModel
    {
        /// <summary>
        /// List of Contact IDs
        /// </summary>
        public IEnumerable<int> Contacts { get; set; }
        /// <summary>
        /// List of Form Submission IDs
        /// </summary>
        public IEnumerable<string> FormSubmissionIds { get; set; }
        /// <summary>
        /// Email Body
        /// </summary>
        public string EmailBody { get; set; }
    }
}
