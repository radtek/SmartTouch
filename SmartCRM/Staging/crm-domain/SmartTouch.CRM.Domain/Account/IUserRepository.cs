using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Account
{
    public interface IUserRepository : IRepository<User, int>
    {
        string GetUserName(int userId);
    }
}
