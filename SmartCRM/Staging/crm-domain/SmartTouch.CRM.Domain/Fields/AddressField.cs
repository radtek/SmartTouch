using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Fields
{
    public class AddressField : Field
    {
        public AddressField()
        {
            FieldCode = "AddressField";
            IList<Field> fields = new List<Field>();
            fields.Add(new AddressLine1Field());
            fields.Add(new AddressLine2Field());
            fields.Add(new CityField());
            fields.Add(new StateField());
            fields.Add(new ZipCodeField());
            fields.Add(new CountryField());
            this.Title = "Address";
            this.FieldCode = "AddressField";
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
    public class AddressLine1Field : Field
    {
        public AddressLine1Field()
        {
            this.Id = (int)ContactFields.AddressLine1Field;
            this.Title = "Address Line1";
            this.FieldCode = "AddressLine1Field";
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
    public class AddressLine2Field : Field
    {
        public AddressLine2Field()
        {
            this.Id = (int)ContactFields.AddressLine2Field;
            this.Title = "Address Line2";
            this.FieldCode = "AddressLine2Field";
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
    public class CityField : Field
    {
        public CityField()
        {
            this.Id = (int)ContactFields.CityField;
            this.Title = "City";
            this.FieldCode = "CityField";
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
    public class StateField : Field
    {
        public StateField()
        {
            this.Id = (int)ContactFields.StateField;
            this.Title = "State";
            this.FieldCode = "StateField";
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
    public class ZipCodeField : Field
    {
        public ZipCodeField()
        {
            this.Id = (int)ContactFields.ZipCodeField;
            this.Title = "ZipCode";
            this.FieldCode = "ZipCodeField";
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
    public class CountryField : Field
    {
        public CountryField()
        {
            this.Id = (int)ContactFields.CountryField;
            this.Title = "Country";
            this.FieldCode = "CountryField";
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
    public class HomeAddressField : AddressField
    {
        public HomeAddressField()
        {
            this.Title = "Home Address";
            this.FieldCode = "HomeAddressField";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }
    public class MailingAddressField : AddressField
    {
        public MailingAddressField()
        {
            this.Title = "Mailing Adress";
            this.FieldCode = "MailingAddressField";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }
    public class WorkAddressField : AddressField
    {
        public WorkAddressField()
        {
            this.Title = "Work Address";
            this.FieldCode = "WorkAddressField";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }
}
