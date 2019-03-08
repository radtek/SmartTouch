using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ContactCustomField : ValueObjectBase
    {
        public int ContactCustomFieldMapId { get; set; }
        public int ContactId { get; set; }
        public int CustomFieldId { get; set; }
        public string Value { get; set; }
        public int FieldInputTypeId { get; set; }

        public DateTime Value_Date
        {
            get
            {
                DateTime date = new DateTime();
                if (this.FieldInputTypeId == (int)FieldType.date || this.FieldInputTypeId == (int)FieldType.datetime || this.FieldInputTypeId == (int)FieldType.time)
                    DateTime.TryParse(this.Value, out date);
                if (this.FieldInputTypeId == (int)FieldType.time)
                {
                    var specifiedDate = date.Date;
                    var dateTicks = specifiedDate.Ticks;
                    var dateTimeTicks = date.Ticks;
                    var aggTicks = dateTimeTicks - dateTicks;
                    date = new DateTime(aggTicks);
                }
                return date;
            }
        }

        public int Value_Number
        {
            get
            {
                int number = 0;
                if (this.FieldInputTypeId == (int)FieldType.number)
                    Int32.TryParse(this.Value, out number);
                return number;
            }
        }

        public List<int> Value_Multiselect
        {
            get
            {
                List<int> Ids = new List<int>();
                if ((this.FieldInputTypeId == (byte)FieldType.multiselectdropdown || this.FieldInputTypeId == (byte)FieldType.checkbox) && !string.IsNullOrEmpty(this.Value))
                {                   
                        string[] sources = this.Value.Split('|');
                        foreach (var id in sources)
                        {
                            int number = 0;
                            Int32.TryParse(id, out number);
                            if (number != 0)
                                Ids.Add(number);
                        }
                        return Ids;                    
                }
                return Ids;
            }
        }

        public int Value_Multiselect_Count
        {
            get
            {
                return this.Value_Multiselect.Count;
            }
        }

        protected override void Validate()
        {
        }
    }
}
