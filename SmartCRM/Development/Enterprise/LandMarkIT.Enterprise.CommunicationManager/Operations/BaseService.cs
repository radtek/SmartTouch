using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using LandmarkIT.Enterprise.CommunicationManager.Repositories;
using System;
using LandmarkIT.Enterprise.Common;
using SmartTouch.CRM.Entities;
using LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    public delegate void ServiceEventHandler(object sender, ServiceEventArgs eventArgs);

    public class ServiceEventArgs: EventArgs
    {
        public ServiceErrorStatus ServiceStatus { get; set; }
    }

    public abstract class BaseService
    {
        public event ServiceEventHandler OnServiceFailure;

        public IUnitOfWork unitOfWork = default(IUnitOfWork);

        protected virtual void OnFailure(object sender, ServiceEventArgs eventArgs)
        {
            ServiceEventHandler handler = OnServiceFailure;
            if (OnServiceFailure != null)
                handler(sender, eventArgs);
        }

        public BaseService()
        {
            this.unitOfWork = new EfUnitOfWork();
        }
        public BaseService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
    }
}
