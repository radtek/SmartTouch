using System.Collections.Generic;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Communication 
{
    public interface ICommunicationRepository : IRepository<CommunicationTracker,int>
    {
      CommunicationTracker FindByContactId(int contactId, CommunicationType communicationType);
    }
}
