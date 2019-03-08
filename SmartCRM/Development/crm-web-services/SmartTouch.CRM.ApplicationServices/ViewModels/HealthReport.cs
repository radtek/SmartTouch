using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ContactGroup
    {
        /// <summary>
        /// 
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Module { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AccountName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int AccountID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? ContactsCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? ElasticCount { get; set; }
    }
}
