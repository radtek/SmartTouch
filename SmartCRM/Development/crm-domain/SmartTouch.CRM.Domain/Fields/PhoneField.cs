using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Fields
{
    public class PhoneField: Field
    {
        
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class HomePhoneField : PhoneField
    {
        public HomePhoneField() {
            this.Id = (int)ContactFields.HomePhoneField;
            this.Title = "Home Phone";
            this.FieldCode = "HomePhoneField";
        }      

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class WorkPhoneField : PhoneField
    {
        public WorkPhoneField() {
            this.Id = (int)ContactFields.WorkPhoneField;
            this.Title = "Work Phone";
            this.FieldCode = "WorkPhoneField";
        }       

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class MobilePhoneField : PhoneField
    {
        public MobilePhoneField()
        {
            this.Id = (int)ContactFields.MobilePhoneField;
            this.Title = "Mobile Phone";
            this.FieldCode = "MobilePhoneField";
        } 

        protected override void Validate()
        {
            base.Validate();
        }
    }
}
