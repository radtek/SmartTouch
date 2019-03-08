
using SmartTouch.CRM.Infrastructure.Domain;
namespace SmartTouch.CRM.Domain.User
{
  public static class UserBusinessRule
    {
        public static readonly BusinessRule UserFirstNameRequired = new BusinessRule("First Name is required.");
        public static readonly BusinessRule UserLastNameRequired = new BusinessRule("Last Name is required");
        public static readonly BusinessRule UserPrimaryEmailRequired = new BusinessRule("Email is invalid");
    }
}
