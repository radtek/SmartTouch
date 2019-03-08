using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Fields
{
    public class NameField : Field
    {
        
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class FirstNameField : NameField
    {
        public FirstNameField()
        {
            this.Id = (int)ContactFields.FirstNameField;
            this.Title = "First Name";
            this.FieldCode = "FirstNameField";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class LastNameField : NameField
    {
        public LastNameField() {
            this.Id = (int)ContactFields.LastNameField;
            this.Title = "Last Name";
            this.FieldCode = "LastNameField";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class CompanyNameField : NameField
    {
        public CompanyNameField() {
            this.Id = (int)ContactFields.CompanyNameField;
            this.Title = "Company";
            this.FieldCode = "CompanyNameField";
        }
      
        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class TitleField : NameField
    {
        public TitleField()
        {
            this.Id = (int)ContactFields.TitleField;
            this.Title = "Title";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }
}
