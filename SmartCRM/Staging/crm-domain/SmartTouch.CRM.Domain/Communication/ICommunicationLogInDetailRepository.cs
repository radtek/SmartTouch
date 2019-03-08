using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Communication
{
    public interface ICommunicationLogInDetailRepository : IRepository<CommunicationLogInDetails, int>
    {
        CommunicationLogInDetails GetCommunicationLogInDetails(int userId, CommunicationType communicationType);
    }
}
