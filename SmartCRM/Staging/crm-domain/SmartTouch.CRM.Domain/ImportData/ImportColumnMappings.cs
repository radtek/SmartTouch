using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImportData
{
    public class ImportColumnMappings : EntityBase<int>, IAggregateRoot
    {
        public string SheetColumnName { get; set; }
        public bool IsCustomField { get; set; }
        public bool IsDropDownField { get; set; }
        public string ContactFieldName { get; set; }
        public int JobID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
