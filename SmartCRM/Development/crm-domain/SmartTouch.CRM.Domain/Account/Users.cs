using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Account
{
    public class User : EntityBase<int>, IAggregateRoot
    {
        string userLoginId;
        public string UserLoginID { get { return userLoginId; } set { userLoginId = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string userName;
        public string UserName { get { return userName; } set { userName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        //For Auto complete in relation
        public string Text { get { return userName; } set { userName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }
        // end

        protected override void Validate()
        {
            throw new NotImplementedException();
        }


    }
}
