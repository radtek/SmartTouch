using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.Domain.User
{
    public interface IUserRepository : IRepository<User, int>
    {
        //string GetUserName(int userId);
        IEnumerable<User> FindAll(string name);
        IEnumerable<User> FindAll(string name, int limit, int pageNumber);
        IEnumerable<Role> GetRoles();

        bool DeactivateUser(int contactId);
        int ChangeRole(int roleId);
        bool IsDuplicateUser(string firstName, string lastName, string primaryEmail, string company, int userId);
    }
}
