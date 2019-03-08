
namespace SmartTouch.CRM.Domain.Contact
{
    public class Company: Contact
    {
        protected override void Validate()
        {
            base.Validate();

            if (HomePhone !=null && !string.IsNullOrEmpty(HomePhone.Number) && !IsValidPhoneNumberLength(HomePhone.Number))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            }

            if (MobilePhone !=null && !string.IsNullOrEmpty(MobilePhone.Number) && !IsValidPhoneNumberLength(MobilePhone.Number))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            }
        }
    }
}
