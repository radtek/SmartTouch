using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Infrastructure.UnitOfWork
{
    public interface IUnitOfWorkRepository
    {
        IAggregateRoot PersistInsertion(IAggregateRoot aggregateRoot);
        IAggregateRoot PersistUpdate(IAggregateRoot aggregateRoot);
        void PersistDeletion(IAggregateRoot aggregateRoot);
    }
}
