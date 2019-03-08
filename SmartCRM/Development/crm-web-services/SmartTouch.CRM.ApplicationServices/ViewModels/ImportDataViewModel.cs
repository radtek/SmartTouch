using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{    
    public class ImportDataViewModel
    {
        public string SheetColumnName { get; set; }
        public string PreviewData { get; set; }
        public bool IsCustomField { get; set; }
        public bool IsDropDownField { get; set; }
        public string ContactFieldName { get; set; }
        public string Title { get; set; }
        public FieldType FieldType { get; set; }
        public int FieldID { get; set; }
    }
}
