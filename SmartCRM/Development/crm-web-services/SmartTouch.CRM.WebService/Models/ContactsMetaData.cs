using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.WebService.Models
{
    public class InputFormat
    {
        public InputFormat(string title, string inputType, string jsonPropertyName, bool isArray = false, string objectType = null, bool isNullable = false, byte dropdownId = 0, string value = "5")
        {
            this.Title = title;
            this.InputType = inputType;
            this.JsonPropertyName = jsonPropertyName;
            this.IsArray = isArray;
            this.ObjectType = objectType;
            this.IsNullable = IsNullable;
            this.DropdownId = dropdownId;
            this.Value = value;
        }
        public string Title { get; set; }
        public string InputType { get; set; }
        public string JsonPropertyName { get; set; }
        public bool IsArray { get; set; }
        public string ObjectType { get; set; }
        private bool IsNullable { get; set; }
        public byte DropdownId { get; set; }
        public string Value { get; set; }
    }

    public class PersonViewModelType
    {
        public PersonViewModelType()
        {
            this.Properties = new List<InputFormat>();
            this.ObjectTypes = new List<InputObjectFormat>();
        }
        public List<InputFormat> Properties { get; set; }
        public List<InputObjectFormat> ObjectTypes { get; set; }
    }

    public class InputObjectFormat
    {
        public InputObjectFormat()
        {
            this.Properties = new List<InputFormat>();
        }

        public string ObjectType { get; set; }
        public List<InputFormat> Properties { get; set; }
    }
}
