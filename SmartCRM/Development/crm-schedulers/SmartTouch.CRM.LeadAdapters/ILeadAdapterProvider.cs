using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.ImportData;

namespace SmartTouch.CRM.LeadAdapters
{
    public interface ILeadAdapterProvider
    {
        void Initialize();
        void Process();        
        ImportContactsData GetContacts(string fileName,IEnumerable<FieldViewModel> customFields,int jobId , IEnumerable<DropdownValueViewModel> DropdownFields);
        bool IsValidPhoneNumberLength(string phoneNumber);   
        Dictionary<string, string> GetFieldMappings();
    }
}
