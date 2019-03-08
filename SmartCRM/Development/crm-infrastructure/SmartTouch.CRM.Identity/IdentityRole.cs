using Microsoft.AspNet.Identity;
using SmartTouch.CRM.Domain.Roles;
using System;

namespace SmartTouch.CRM.Identity
{
    public class IdentityRole : Role, IRole
    {
        public new string Id { get { return Id.ToString(); } }

        public string Name { get; set; }
    }
}
