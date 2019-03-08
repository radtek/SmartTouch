using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA = SmartTouch.CRM.Domain.Action;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain;

namespace SmartTouch.CRM.Domain.Action
{
    public interface IActionRepository : IRepository<Action, int>
    {
        Action FindBy(int actionId, int contactId);
        IEnumerable<Action> FindByContact(int contactId);
        Tag.Tag FindTag(string actionName);
        void ActionCompleted(int actionId, bool status,int contactId);
        void DeleteActionForAll(int actionId);
        int ContactsCount(int actionId);
    }
}
